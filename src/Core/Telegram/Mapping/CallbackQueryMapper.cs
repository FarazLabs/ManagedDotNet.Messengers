using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ManagedDotNet.Messengers.Core.Telegram.Mapping;

internal sealed class CallbackQueryMapper : TelegramMapper<CallbackQuery>
{
    public override TelegramMappedUpdate? Adapt(UpdateType updateType, CallbackQuery callbackQuery)
    {
        if (callbackQuery.Message is null)
            return null;

        var chat = new ChatData
        {
            Source = Messenger.Telegram,
            ChatId = callbackQuery.Message.Chat.Id.ToString(),
        };

        var user = new UserData
        {
            AccountId = callbackQuery.From.Id.ToString(),
            UserName = callbackQuery.From.Username,
            Name = $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName}".Trim(),
            Language = callbackQuery.From.LanguageCode,
        };

        return new TelegramMappedUpdate(chat, user, new ButtonClicked { Metadata = callbackQuery.Data });
    }
}
