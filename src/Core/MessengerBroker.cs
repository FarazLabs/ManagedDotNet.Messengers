using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Core;

/// <inheritdoc cref="IMessengerBroker" />
internal sealed class MessengerBroker : IMessengerBroker
{
    private readonly IReadOnlyList<IMessengerTransport> _messengerTransports;

    public MessengerBroker(IEnumerable<IMessengerTransport> messengers)
    {
        _messengerTransports = messengers.ToList();
    }

    public Task Send(ChatData chat, string text)
    {
        var client = Resolve(chat.Source);
        return client.SendAsync(chat.ChatId, text);
    }

    public Task Send(ChatData chat, string text, List<Button> buttons)
    {
        var client = Resolve(chat.Source);
        return client.SendAsync(chat.ChatId, text, buttons);
    }

    private IMessengerTransport Resolve(Messenger messenger) =>
        _messengerTransports.Single(m => m.Messenger == messenger);
}
