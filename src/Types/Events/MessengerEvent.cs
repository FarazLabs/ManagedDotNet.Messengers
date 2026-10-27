using ManagedDotNet.Messengers.Abstractions;

namespace ManagedDotNet.Messengers.Types.Events;

/// <summary>
/// Base type for every inbound messenger payload handled by <see cref="IMessengerMessageHandler{TMessage}"/>.
/// </summary>
public abstract class MessengerEvent;
