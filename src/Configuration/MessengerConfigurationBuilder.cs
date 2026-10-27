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
    /// Binds <typeparamref name="TEvent"/> on this messenger to <typeparamref name="TEventHandler"/>.
    /// </summary>
    /// <exception cref="MisconfiguredException">
    /// <typeparamref name="TEvent"/> already has a handler on this messenger.
    /// </exception>
    public MessengerConfigurationBuilder UseHandler<TEvent, TEventHandler>()
        where TEvent : MessengerEvent
        where TEventHandler : class, IMessengerMessageHandler<TEvent>
    {
        _registry.AddBinding(Messenger, typeof(TEvent), typeof(TEventHandler));
        Services.TryAddScoped<TEventHandler>();
        return this;
    }

    /// <summary>
    /// Registers <typeparamref name="TInterceptor"/> for this messenger. Interceptors run in registration
    /// order, then the typed handler. The same <see cref="Types.Models.EventEnvelope"/> instance is shared.
    /// </summary>
    public MessengerConfigurationBuilder UseInterceptor<TInterceptor>()
        where TInterceptor : class, IMessengerEventInterceptor
    {
        _registry.AddInterceptor(Messenger, typeof(TInterceptor));
        Services.TryAddScoped<TInterceptor>();
        return this;
    }
}
