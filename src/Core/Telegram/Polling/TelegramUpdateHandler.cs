using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Core.Telegram.Mapping;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ManagedDotNet.Messengers.Core.Telegram.Polling;

/// <summary>
/// Maps Telegram updates to chat/user/event and dispatches them to the registered handler.
/// </summary>
internal sealed class TelegramUpdateHandler : IUpdateHandler
{
    private readonly ILogger<TelegramUpdateHandler> _logger;
    private readonly IMessengerEventDispatcher _dispatcher;

    public TelegramUpdateHandler(ILogger<TelegramUpdateHandler> logger, IMessengerEventDispatcher dispatcher)
    {
        _logger = logger;
        _dispatcher = dispatcher;
    }

    public async Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogCritical(exception, "Telegram error handler ({Source})", source);

        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Exactly one property besides Id is set on an Update; that is the payload.
        object payload = typeof(Update)
            .GetProperties()
            .Where(p => p.Name != nameof(Update.Id))
            .Select(p => p.GetValue(update))
            .First(v => v is not null)!;

        TelegramMappedUpdate? mapped = TelegramMapperRegistry.FetchMapper(payload.GetType())?.Map(update.Type, payload);
        if (mapped is null)
        {
            _logger.LogDebug("Unsupported Telegram update {UpdateId} of type {UpdateType}", update.Id, update.Type);
            return Task.CompletedTask;
        }

        string correlationId = update.Id.ToString();

        _ = Task.Run(async () =>
        {
            try
            {
                await _dispatcher.DispatchAsync(
                    correlationId,
                    mapped.Message,
                    mapped.Chat,
                    mapped.User,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "Handler for {MessageType} failed on Telegram update {UpdateId}",
                    mapped.Message.GetType().Name, update.Id);
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }
}
