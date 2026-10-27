using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedDotNet.Messengers.Core;

/// <summary>
/// Resolves interceptors and the handler registered for an inbound message and invokes them in one DI scope.
/// </summary>
internal sealed class MessengerEventDispatcher : IMessengerEventDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MessengerEventHandlerRegistry _registry;
    private readonly ILogger<MessengerEventDispatcher> _logger;

    public MessengerEventDispatcher(
        IServiceScopeFactory scopeFactory,
        MessengerEventHandlerRegistry registry,
        ILogger<MessengerEventDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _registry = registry;
        _logger = logger;
    }

    public async Task DispatchAsync(
        string correlationId,
        MessengerEvent message,
        ChatData chatData,
        UserData userData,
        CancellationToken cancellationToken)
    {
        Type messageType = message.GetType();

        if (!_registry.TryGetBinding(chatData.Source, messageType, out MessengerHandlerBinding binding))
        {
            _logger.LogDebug(
                "No handler registered for {MessageType} on {Messenger}; skipping update {CorrelationId}",
                messageType.Name, chatData.Source, correlationId);
            return;
        }

        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        IServiceProvider sp = scope.ServiceProvider;

        object handler = sp.GetRequiredService(binding.HandlerType);
        EventEnvelope envelope = binding.CreateEnvelope(message, chatData, userData, correlationId);

        IReadOnlyList<Type> interceptorTypes = _registry.GetInterceptors(chatData.Source);
        IMessengerEventInterceptor[] interceptors = new IMessengerEventInterceptor[interceptorTypes.Count];
        for (int i = 0; i < interceptorTypes.Count; i++)
            interceptors[i] = (IMessengerEventInterceptor)sp.GetRequiredService(interceptorTypes[i]);

        Func<Task> next = () => binding.Invoke(handler, envelope, cancellationToken);

        for (int i = interceptors.Length - 1; i >= 0; i--)
        {
            IMessengerEventInterceptor interceptor = interceptors[i];
            Func<Task> continuePipeline = next;
            next = () => interceptor.Intercept(envelope, continuePipeline, cancellationToken);
        }

        await next();
    }
}
