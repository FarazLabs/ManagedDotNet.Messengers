using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ManagedDotNet.Messengers.Core.Telegram.Mapping;

internal sealed class MessageMapper : TelegramMapper<Message>
{
    public override TelegramMappedUpdate? Adapt(UpdateType updateType, Message msg)
    {
        if (msg.From is null)
            return null;

        MessengerEvent? message = (updateType, msg.Type) switch
        {
            (UpdateType.Message, MessageType.Text) => new TextMessageReceived
            {
                MessageId = msg.MessageId.ToString(),
                Content = msg.Text!,
            },
            (UpdateType.Message, MessageType.Voice) => new VoiceMessageReceived
            {
                FileId = msg.Voice!.FileId,
            },
            (UpdateType.EditedMessage, MessageType.Text) => new TextMessageEdited
            {
                MessageId = msg.MessageId.ToString(),
                Content = msg.Text!,
            },
            _ => null,
        };

        if (message is null)
            return null;

        var chat = new ChatData
        {
            Source = Messenger.Telegram,
            ChatId = msg.Chat.Id.ToString(),
        };

        var user = new UserData
        {
            AccountId = msg.From.Id.ToString(),
            UserName = msg.From.Username,
            Name = $"{msg.From.FirstName} {msg.From.LastName}".Trim(),
            Language = msg.From.LanguageCode,
        };

        return new TelegramMappedUpdate(chat, user, message);
    }
}
