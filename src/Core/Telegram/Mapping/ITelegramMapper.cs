using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;
using Telegram.Bot.Types.Enums;

namespace ManagedDotNet.Messengers.Core.Telegram.Mapping;

internal sealed record TelegramMappedUpdate(ChatData Chat, UserData User, MessengerEvent Message);

internal interface ITelegramMapper
{
    /// <summary>
    /// Different update types are possible for one payload type
    /// (e.g. <c>Message</c> for both <see cref="UpdateType.Message"/> and <see cref="UpdateType.EditedMessage"/>).
    /// </summary>
    /// <returns><c>null</c> when the update is not supported.</returns>
    TelegramMappedUpdate? Map(UpdateType updateType, object update);
}

internal abstract class TelegramMapper<TUpdate> : ITelegramMapper
{
    public TelegramMappedUpdate? Map(UpdateType updateType, object update)
    {
        if (update is TUpdate typedUpdate)
            return Adapt(updateType, typedUpdate);

        throw new ArgumentException($"Invalid update type: expected {typeof(TUpdate)}, got {update.GetType()}");
    }

    public abstract TelegramMappedUpdate? Adapt(UpdateType updateType, TUpdate update);
}
