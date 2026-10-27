using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Abstractions;

/// <summary>
/// Routes outbound messages to the registered <see cref="IMessengerTransport"/> for the target channel.
/// </summary>
public interface IMessengerBroker
{
    Task Send(ChatData chat, string text);

    Task Send(ChatData chat, string text, List<Button> buttons);
}
