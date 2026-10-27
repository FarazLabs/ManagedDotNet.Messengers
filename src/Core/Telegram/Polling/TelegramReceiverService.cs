using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace ManagedDotNet.Messengers.Core.Telegram.Polling;

internal sealed class TelegramReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TelegramUpdateHandler _updateHandler;
    private readonly ILogger<TelegramReceiverService> _logger;

    public TelegramReceiverService(
        ITelegramBotClient botClient,
        TelegramUpdateHandler updateHandler,
        ILogger<TelegramReceiverService> logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public async Task ReceiveAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            DropPendingUpdates = true,
        };

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation(
            "Telegram receiver started for bot {BotName}",
            me.Username ?? "unknown");

        await _botClient.ReceiveAsync(_updateHandler, receiverOptions, stoppingToken);
    }
}
