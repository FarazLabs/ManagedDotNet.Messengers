using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ManagedDotNet.Messengers.Core.Telegram;

/// <summary>
/// Telegram Bot API implementation of <see cref="IMessengerTransport"/>.
/// </summary>
internal sealed class TelegramMessengerGateway : IMessengerTransport
{
    private readonly ITelegramBotClient _bot;

    public TelegramMessengerGateway(ITelegramBotClient bot)
    {
        _bot = bot;
    }


    public Messenger Messenger => Messenger.Telegram;

    public async Task SendAsync(string chatId, string text)
    {
        await _bot.SendMessage(chatId, text);
    }


    public async Task SendAsync(string chatId, string text, List<Button> buttons)
    {
        var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();

        foreach (var button in buttons)
        {
            var buttonRow = new List<InlineKeyboardButton>();

            if (button.Type == ButtonType.Url)
            {
                buttonRow.Add(InlineKeyboardButton.WithWebApp(text: button.Text, new WebAppInfo(button.Metadata)));
            }
            else if (button.Type == ButtonType.CallBackQuery)
            {
                buttonRow.Add(InlineKeyboardButton.WithCallbackData(text: button.Text, callbackData: button.Metadata));
            }

            inlineKeyboardButtons.Add(buttonRow);
        }

        var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);

        await _bot.SendMessage(
            chatId: chatId,
            text: text,
            replyMarkup: inlineKeyboard);
    }


}
