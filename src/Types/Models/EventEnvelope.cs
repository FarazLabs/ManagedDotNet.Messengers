using ManagedDotNet.Messengers.Types.Events;

namespace ManagedDotNet.Messengers.Types.Models;

/// <summary>
/// Wraps an inbound messenger event with user, chat, and correlation metadata.
/// </summary>
public class EventEnvelope<T> where T : MessengerEvent
{
    public T Event { get; set; } = default!;

    public UserData User { get; set; } = default!;

    public ChatData Chat { get; set; } = default!;

    /// <summary>
    /// Platform update id, used to correlate logs and retries.
    /// </summary>
    public string CorrelationId { get; set; } = default!;
}
