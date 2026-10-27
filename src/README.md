# ManagedDotNet.Messengers

Channel-agnostic messenger contracts, typed inbound event handlers registered per messenger, and a Telegram Bot API adapter with long-polling.

| Folder | Namespace | Role |
| --- | --- | --- |
| `Abstractions/` | `.Abstractions` | `IMessengerBroker`, `IMessengerMessageHandler<T>`, `IMessengerEventHandler`, `IMessengerEventInterceptor` |
| `Types/Events/` | `.Types.Events` | Inbound events: `TextMessageReceived`, `VoiceMessageReceived`, `ButtonClicked`, `TextMessageEdited` |
| `Types/Models/` | `.Types.Models` | `EventEnvelope`, `EventEnvelope<T>`, `ChatData`, `UserData`, `Button` |
| `Core/` | `.Core` | `MessengerBroker`, dispatcher, Telegram transport/polling/mappers |
| `Configuration/` | `.Configuration` | `AddTelegram(...)`, `UseHandler<TEvent, TEventHandler>()`, `UseInterceptor<TInterceptor>()`, options |
| `Types/Enums/` | `.Types.Enums` | `Messenger` |
| `Types/Exceptions/` | `.Types.Exceptions` | `MisconfiguredException` |

## Install

```bash
dotnet add package ManagedDotNet.Messengers
```

## Host setup

Bind each event type to a handler explicitly. Interceptors are optional and run in registration order before the handler:

```csharp
using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Types.Events;

builder.Services.AddTelegram(builder.Configuration)   // or AddTelegram(o => o.BotToken = "...")
    .UseInterceptor<LoggingInterceptor>()
    .UseHandler<TextMessageReceived, TextMessageHandler>()
    .UseHandler<TextMessageEdited, TelegramEditTextMessageHandler>()
    .UseHandler<VoiceMessageReceived, MediaHandler>()
    .UseHandler<ButtonClicked, MediaHandler>();
```

```json
{
  "TelegramOptions": {
    "BotToken": "<your-bot-token>"
  }
}
```

`AddTelegram` also registers `IMessengerBroker` for outbound messages.

## Inbound: handlers

Implement `IMessengerMessageHandler<TEvent>` for each event type the class handles, then register each pair with `UseHandler<TEvent, TEventHandler>()`.

```csharp
using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;

public sealed class TextMessageHandler : IMessengerMessageHandler<TextMessageReceived>
{
    public Task Handle(EventEnvelope<TextMessageReceived> envelope, CancellationToken ct)
    {
        // envelope.Event.Content
        // envelope.Chat.ChatId / envelope.Chat.Source
        // envelope.User.AccountId
        // envelope.CorrelationId
        return Task.CompletedTask;
    }
}

public sealed class MediaHandler :
    IMessengerMessageHandler<VoiceMessageReceived>,
    IMessengerMessageHandler<ButtonClicked>
{
    public Task Handle(EventEnvelope<VoiceMessageReceived> envelope, CancellationToken ct) => Task.CompletedTask;
    public Task Handle(EventEnvelope<ButtonClicked> envelope, CancellationToken ct) => Task.CompletedTask;
}
```

## Inbound: interceptors

Implement `IMessengerEventInterceptor` (not tied to an event type). Register with `UseInterceptor<TInterceptor>()`. Interceptors receive the same `EventEnvelope` instance the handler will get. Call `next` to continue; omit it to short-circuit.

```csharp
using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Models;

public sealed class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : IMessengerEventInterceptor
{
    public async Task Intercept(EventEnvelope envelope, Func<Task> next, CancellationToken ct)
    {
        logger.LogInformation("Inbound {EventType} {CorrelationId}", envelope.Event.GetType().Name, envelope.CorrelationId);
        await next();
    }
}
```

`EventEnvelope` / `EventEnvelope<T>` carries:

| Property | Meaning |
| --- | --- |
| `Event` | Inbound payload (`MessengerEvent` on the base; typed on `EventEnvelope<T>`) |
| `User` | `AccountId`, `UserName`, `Name`, `Picture`, `Language` |
| `Chat` | `Source` (messenger) and `ChatId` — also used for outbound `IMessengerBroker.Send` |
| `CorrelationId` | Platform update id (logs / retries) |

Rules:

- Handlers and interceptors are registered with `TryAddScoped` (safe to bind the same type more than once) and resolved in a fresh scope per update.
- One handler per event type per messenger. A second binding for the same pair throws `MisconfiguredException`. Calling `AddTelegram` twice also throws.
- Interceptors for a messenger run in registration order, then the typed handler. The dispatcher builds one envelope and shares it through the pipeline.
- At host start, any concrete `MessengerEvent` with no handler on a registered messenger logs a **Warning** (does not throw). Those updates are skipped at Debug when they arrive.
- Each update runs in the background; handler exceptions are logged and do not stop polling.

## Outbound

```csharp
using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Models;

await broker.Send(
    new ChatData { Source = Messenger.Telegram, ChatId = envelope.Chat.ChatId },
    "Hello");

await broker.Send(
    new ChatData { Source = Messenger.Telegram, ChatId = envelope.Chat.ChatId },
    "Pick one",
    new List<Button>
    {
        new() { Type = ButtonType.CallBackQuery, Text = "Yes", Metadata = "yes" },
    });
```
