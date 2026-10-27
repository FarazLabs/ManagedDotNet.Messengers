using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Abstractions;

/// <summary>
/// Dispatches a <see cref="MessengerEvent"/> to the registered handler as per configuration.
/// </summary>
internal interface IMessengerEventDispatcher
{
    Task DispatchAsync(
        string correlationId,
        MessengerEvent message,
        ChatData chatData,
        UserData userData,
        CancellationToken cancellationToken);
}
