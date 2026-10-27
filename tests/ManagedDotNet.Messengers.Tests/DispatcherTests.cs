using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Tests.Fixtures;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedDotNet.Messengers.Tests;

public class DispatcherTests
{
    private static (ServiceProvider Provider, Recorder Recorder) Build(Action<MessengerConfigurationBuilder> configure)
    {
        var recorder = new Recorder();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(recorder);
        configure(services.AddTelegram(o => o.BotToken = "test-token"));
        return (services.BuildServiceProvider(), recorder);
    }

    private static ChatData TelegramChat() => new()
    {
        Source = Messenger.Telegram,
        ChatId = "42",
    };

    private static UserData TelegramUser() => new()
    {
        AccountId = "7",
    };

    [Fact]
    public async Task Invokes_registered_handler_with_message_and_context()
    {
        var (provider, recorder) = Build(b => b
            .WithEventHandler<TextMessageReceived, TextHandler>()
            .WithEventHandler<VoiceMessageReceived, MediaHandler>()
            .WithEventHandler<ButtonClicked, MediaHandler>());
        var dispatcher = provider.GetRequiredService<IMessengerEventDispatcher>();
        var chat = TelegramChat();
        var user = TelegramUser();
        var message = new TextMessageReceived { MessageId = "5", Content = "hi" };

        await dispatcher.DispatchAsync("1001", message, chat, user, CancellationToken.None);

        var call = Assert.Single(recorder.Calls);
        Assert.Same(message, call.Message);
        Assert.Same(chat, call.Envelope.Chat);
        Assert.Same(user, call.Envelope.User);
        Assert.Equal("1001", call.Envelope.CorrelationId);
        Assert.Equal(typeof(TextHandler), call.Handler);
    }

    [Fact]
    public async Task Routes_each_message_type_of_a_multi_handler()
    {
        var (provider, recorder) = Build(b => b
            .WithEventHandler<VoiceMessageReceived, MediaHandler>()
            .WithEventHandler<ButtonClicked, MediaHandler>());
        var dispatcher = provider.GetRequiredService<IMessengerEventDispatcher>();

        await dispatcher.DispatchAsync("1", new VoiceMessageReceived { FileId = "f" }, TelegramChat(), TelegramUser(), CancellationToken.None);
        await dispatcher.DispatchAsync("2", new ButtonClicked { Metadata = "yes" }, TelegramChat(), TelegramUser(), CancellationToken.None);

        Assert.Collection(recorder.Calls,
            c => Assert.IsType<VoiceMessageReceived>(c.Message),
            c => Assert.IsType<ButtonClicked>(c.Message));
    }

    [Fact]
    public async Task Skips_message_without_handler()
    {
        var (provider, recorder) = Build(b => b
            .WithEventHandler<VoiceMessageReceived, MediaHandler>()
            .WithEventHandler<ButtonClicked, MediaHandler>());
        var dispatcher = provider.GetRequiredService<IMessengerEventDispatcher>();

        await dispatcher.DispatchAsync(
            "1",
            new TextMessageReceived { MessageId = "1", Content = "x" },
            TelegramChat(),
            TelegramUser(),
            CancellationToken.None);

        Assert.Empty(recorder.Calls);
    }
}
