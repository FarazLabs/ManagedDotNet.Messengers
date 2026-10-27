using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ManagedDotNet.Messengers.Configuration;

/// <summary>
/// Configures inbound handlers for one messenger. Returned by <c>AddTelegram(...)</c>.
/// </summary>
public sealed class MessengerConfigurationBuilder
{
    private readonly MessengerEventHandlerRegistry _registry;

    internal MessengerConfigurationBuilder(IServiceCollection services, Messenger messenger, MessengerEventHandlerRegistry registry)
    {
        Services = services;
        Messenger = messenger;
        _registry = registry;
    }

    public IServiceCollection Services { get; }

    public Messenger Messenger { get; }

    /// <summary>
    /// Binds <typeparamref name="TEvent"/> on this messenger to <typeparamref name="THandler"/>.
    /// </summary>
    /// <exception cref="MisconfiguredException">
    /// <typeparamref name="TEvent"/> already has a handler on this messenger.
    /// </exception>
    public MessengerConfigurationBuilder WithEventHandler<TEvent, THandler>()
        where TEvent : MessengerEvent
        where THandler : class, IMessengerMessageHandler<TEvent>
    {
        _registry.AddBinding(Messenger, typeof(TEvent), typeof(THandler));
        Services.TryAddScoped<THandler>();
        return this;
    }
}
