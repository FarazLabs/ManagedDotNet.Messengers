using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedDotNet.Messengers.Core;

/// <summary>
/// Resolves the handler registered for an inbound message and invokes it in its own DI scope.
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
        object handler = scope.ServiceProvider.GetRequiredService(binding.HandlerType);
        await binding.Invoke(handler, message, chatData, userData, correlationId, cancellationToken);
    }
}
