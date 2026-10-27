namespace ManagedDotNet.Messengers.Types.Events;

public sealed class TextMessageEdited : MessengerEvent
{
    public string MessageId { get; init; } = default!;
    public string Content { get; init; } = default!;
}
