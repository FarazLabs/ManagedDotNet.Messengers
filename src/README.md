# ManagedDotNet.Messengers

Channel-agnostic messenger contracts, typed inbound event handlers registered per messenger, and a Telegram Bot API adapter with long-polling.

| Folder | Namespace | Role |
| --- | --- | --- |
| `Abstractions/` | `.Abstractions` | `IMessengerBroker`, `IMessengerMessageHandler<T>`, `IMessengerEventHandler` |
| `Types/Events/` | `.Types.Events` | Inbound events: `TextMessageReceived`, `VoiceMessageReceived`, `ButtonClicked`, `TextMessageEdited` |
| `Types/Models/` | `.Types.Models` | `EventEnvelope<T>`, `ChatData`, `UserData`, `Button` |
| `Core/` | `.Core` | `MessengerBroker`, dispatcher, Telegram transport/polling/mappers |
| `Configuration/` | `.Configuration` | `AddTelegram(...)`, `WithEventHandler<TEvent, THandler>()`, options |
| `Types/Enums/` | `.Types.Enums` | `Messenger` |
| `Types/Exceptions/` | `.Types.Exceptions` | `MisconfiguredException` |

## Install

```bash
dotnet add package ManagedDotNet.Messengers
```

## Host setup

Bind each event type to a handler explicitly:

```csharp
using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Types.Events;

builder.Services.AddTelegram(builder.Configuration)   // or AddTelegram(o => o.BotToken = "...")
    .WithEventHandler<TextMessageReceived, TextMessageHandler>()
    .WithEventHandler<TextMessageEdited, TelegramEditTextMessageHandler>()
    .WithEventHandler<VoiceMessageReceived, MediaHandler>()
    .WithEventHandler<ButtonClicked, MediaHandler>();
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

Implement `IMessengerMessageHandler<TEvent>` for each event type the class handles, then register each pair with `WithEventHandler<TEvent, THandler>()`.

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

`EventEnvelope<T>` carries:

| Property | Meaning |
| --- | --- |
| `Event` | Typed inbound payload |
| `User` | `AccountId`, `UserName`, `Name`, `Picture`, `Language` |
| `Chat` | `Source` (messenger) and `ChatId` — also used for outbound `IMessengerBroker.Send` |
| `CorrelationId` | Platform update id (logs / retries) |

Rules:

- Handlers are registered with `TryAddScoped` (safe to bind the same handler type to several events or messengers) and resolved in a fresh scope per update.
- One handler per event type per messenger. A second binding for the same pair throws `MisconfiguredException`. Calling `AddTelegram` twice also throws.
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
