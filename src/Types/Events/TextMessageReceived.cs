namespace ManagedDotNet.Messengers.Types.Events;

public sealed class TextMessageReceived : MessengerEvent
{
    public string MessageId { get; init; } = default!;
    public string Content { get; init; } = default!;
}
