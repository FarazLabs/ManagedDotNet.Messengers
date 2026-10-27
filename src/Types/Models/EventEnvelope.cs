using ManagedDotNet.Messengers.Types.Events;

namespace ManagedDotNet.Messengers.Types.Models;

/// <summary>
/// Wraps an inbound messenger event with user, chat, and correlation metadata.
/// Interceptors receive this non-generic form; handlers receive <see cref="EventEnvelope{T}"/>.
/// </summary>
public class EventEnvelope
{
    public MessengerEvent Event { get; set; } = default!;

    public UserData User { get; set; } = default!;

    public ChatData Chat { get; set; } = default!;

    /// <summary>
    /// Platform update id, used to correlate logs and retries.
    /// </summary>
    public string CorrelationId { get; set; } = default!;
}

/// <summary>
/// Typed envelope for a concrete <typeparamref name="T"/> event. Shares identity with the non-generic base
/// so interceptors and handlers see the same instance.
/// </summary>
public class EventEnvelope<T> : EventEnvelope where T : MessengerEvent
{
    public new T Event
    {
        get => (T)base.Event;
        set => base.Event = value;
    }
}
