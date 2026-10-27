using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Abstractions;

/// <summary>
/// Marker for messenger event handlers.
/// Implement <see cref="IMessengerMessageHandler{TEvent}"/> and register with
/// <c>WithEventHandler&lt;TEvent, THandler&gt;()</c>.
/// </summary>
public interface IMessengerEventHandler;


/// <summary>
/// Handles incoming messenger events. <br /> 
/// A handler may handle multiple event types by implementing this interface for each supported type.
/// </summary>
/// <typeparam name="TEvent">The type of messenger event to handle.</typeparam>
public interface IMessengerMessageHandler<TEvent> : IMessengerEventHandler 
    where TEvent : MessengerEvent
{
    /// <param name="envelope">The event and its associated user and chat context.</param>
    /// <param name="cancellationToken">A token that signals cancellation, typically during host shutdown.</param>
    Task Handle(EventEnvelope<TEvent> envelope, CancellationToken cancellationToken);
}