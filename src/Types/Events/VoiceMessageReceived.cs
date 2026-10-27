namespace ManagedDotNet.Messengers.Types.Events;

public sealed class VoiceMessageReceived : MessengerEvent
{
    public string FileId { get; init; } = default!;
}
