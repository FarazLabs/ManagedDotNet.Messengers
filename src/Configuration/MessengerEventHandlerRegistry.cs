using System.Reflection;
using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Exceptions;
using ManagedDotNet.Messengers.Types.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedDotNet.Messengers.Configuration;

internal sealed record MessengerHandlerBinding(
    Type HandlerType,
    Func<object, MessengerEvent, ChatData, UserData, string, CancellationToken, Task> Invoke);

/// <summary>
/// Singleton map of <c>(messenger, message type)</c> to handler, filled at startup by <see cref="MessengerConfigurationBuilder"/>.
/// </summary>
internal sealed class MessengerEventHandlerRegistry
{
    private static readonly MethodInfo s_invokeHandler =
        typeof(MessengerEventHandlerRegistry).GetMethod(nameof(InvokeHandler), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly HashSet<Messenger> _messengers = new();
    private readonly Dictionary<(Messenger Messenger, Type MessageType), MessengerHandlerBinding> _bindings = new();

    private static readonly Lazy<Type[]> s_concreteEventTypes = new(() =>
        typeof(MessengerEvent).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(MessengerEvent).IsAssignableFrom(t))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToArray());

    internal static MessengerEventHandlerRegistry GetOrCreate(IServiceCollection services)
    {
        ServiceDescriptor? existing = services.FirstOrDefault(d =>
            d.ServiceType == typeof(MessengerEventHandlerRegistry)
            && d.ImplementationInstance is MessengerEventHandlerRegistry);

        if (existing?.ImplementationInstance is MessengerEventHandlerRegistry registry)
            return registry;

        var created = new MessengerEventHandlerRegistry();
        services.AddSingleton(created);
        return created;
    }

    internal void AddMessenger(Messenger messenger)
    {
        if (!_messengers.Add(messenger))
            throw new MisconfiguredException($"Messenger '{messenger}' is already registered. Call Add{messenger}(...) once.");
    }

    internal void AddBinding(Messenger messenger, Type messageType, Type handlerType)
    {
        if (_bindings.TryGetValue((messenger, messageType), out MessengerHandlerBinding? existing))
        {
            throw new MisconfiguredException(
                $"Message type '{messageType.Name}' on '{messenger}' is already handled by '{existing.HandlerType.Name}'; " +
                $"cannot also register '{handlerType.Name}'.");
        }

        var invoke = s_invokeHandler
            .MakeGenericMethod(messageType)
            .CreateDelegate<Func<object, MessengerEvent, ChatData, UserData, string, CancellationToken, Task>>();

        _bindings[(messenger, messageType)] = new MessengerHandlerBinding(handlerType, invoke);
    }

    internal bool TryGetBinding(Messenger messenger, Type messageType, out MessengerHandlerBinding binding) =>
        _bindings.TryGetValue((messenger, messageType), out binding!);

    /// <summary>
    /// For each registered messenger, returns concrete <see cref="MessengerEvent"/> types with no handler binding.
    /// </summary>
    internal IReadOnlyList<(Messenger Messenger, IReadOnlyList<Type> UncoveredEvents)> GetUncoveredEvents()
    {
        var result = new List<(Messenger, IReadOnlyList<Type>)>();

        foreach (Messenger messenger in _messengers.OrderBy(m => m.ToString(), StringComparer.Ordinal))
        {
            Type[] missing = s_concreteEventTypes.Value
                .Where(eventType => !_bindings.ContainsKey((messenger, eventType)))
                .ToArray();

            if (missing.Length > 0)
                result.Add((messenger, missing));
        }

        return result;
    }

    private static Task InvokeHandler<TMessage>(
        object handler,
        MessengerEvent message,
        ChatData chatData,
        UserData userData,
        string correlationId,
        CancellationToken cancellationToken)
        where TMessage : MessengerEvent =>
        ((IMessengerMessageHandler<TMessage>)handler).Handle(
            new EventEnvelope<TMessage>
            {
                Event = (TMessage)message,
                User = userData,
                Chat = chatData,
                CorrelationId = correlationId,
            },
            cancellationToken);
}
