# ManagedDotNet.Messengers

Messenger abstractions and adapters for .NET (FarazLabs): register a messenger, bind typed event handlers with `UseHandler<TEvent, TEventHandler>()`, optional `UseInterceptor<TInterceptor>()`, send through `IMessengerBroker`.

```bash
dotnet add package ManagedDotNet.Messengers
```

```csharp
using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Types.Events;

services.AddTelegram(o => o.BotToken = "...")
    .UseInterceptor<LoggingInterceptor>()
    .UseHandler<TextMessageReceived, TextMessageHandler>()
    .UseHandler<TextMessageEdited, TelegramEditTextMessageHandler>()
    .UseHandler<VoiceMessageReceived, MediaHandler>()
    .UseHandler<ButtonClicked, MediaHandler>();
```

Uncovered concrete event types log a **Warning** at host start (no throw). Full docs: [src/README.md](src/README.md).

| | |
| --- | --- |
| Library docs | [src/README.md](src/README.md) |
| Tests | [tests/ManagedDotNet.Messengers.Tests](tests/ManagedDotNet.Messengers.Tests) |
| License | [MIT](LICENSE) |

Requires .NET 8+.
