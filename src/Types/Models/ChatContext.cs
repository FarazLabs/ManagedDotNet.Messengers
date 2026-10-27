using ManagedDotNet.Messengers.Types.Enums;

namespace ManagedDotNet.Messengers.Types.Models;

/// <summary>
/// Chat destination/source: messenger channel and chat id.
/// </summary>
public class ChatData
{
    public Messenger Source { get; init; }
    public string ChatId { get; init; } = default!;
}
