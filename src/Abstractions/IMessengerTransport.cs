using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Abstractions;

internal interface IMessengerTransport
{
    Messenger Messenger { get; }

    Task SendAsync(string chatId, string text);

    Task SendAsync(
        string chatId,
        string text,
        List<Button> buttons);
}
