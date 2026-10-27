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
            .UseHandler<TextMessageReceived, TextHandler>()
            .UseHandler<VoiceMessageReceived, MediaHandler>()
            .UseHandler<ButtonClicked, MediaHandler>());
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
            .UseHandler<VoiceMessageReceived, MediaHandler>()
            .UseHandler<ButtonClicked, MediaHandler>());
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
            .UseHandler<VoiceMessageReceived, MediaHandler>()
            .UseHandler<ButtonClicked, MediaHandler>());
        var dispatcher = provider.GetRequiredService<IMessengerEventDispatcher>();

        await dispatcher.DispatchAsync(
            "1",
            new TextMessageReceived { MessageId = "1", Content = "x" },
            TelegramChat(),
            TelegramUser(),
            CancellationToken.None);

        Assert.Empty(recorder.Calls);
    }

    [Fact]
    public async Task Runs_interceptors_in_registration_order_with_same_envelope()
    {
        var (provider, recorder) = Build(b => b
            .UseInterceptor<FirstInterceptor>()
            .UseInterceptor<SecondInterceptor>()
            .UseHandler<TextMessageReceived, TextHandler>());
        var dispatcher = provider.GetRequiredService<IMessengerEventDispatcher>();

        await dispatcher.DispatchAsync(
            "1001",
            new TextMessageReceived { MessageId = "5", Content = "hi" },
            TelegramChat(),
            TelegramUser(),
            CancellationToken.None);

        Assert.Collection(recorder.InterceptorCalls,
            c => Assert.Equal("first", c.Name),
            c => Assert.Equal("second", c.Name));

        var handlerCall = Assert.Single(recorder.Calls);
        Assert.Same(handlerCall.Envelope, recorder.InterceptorCalls[0].Envelope);
        Assert.Same(handlerCall.Envelope, recorder.InterceptorCalls[1].Envelope);
    }

    [Fact]
    public async Task Short_circuit_interceptor_skips_handler()
    {
        var (provider, recorder) = Build(b => b
            .UseInterceptor<ShortCircuitInterceptor>()
            .UseHandler<TextMessageReceived, TextHandler>());
        var dispatcher = provider.GetRequiredService<IMessengerEventDispatcher>();

        await dispatcher.DispatchAsync(
            "1",
            new TextMessageReceived { MessageId = "1", Content = "x" },
            TelegramChat(),
            TelegramUser(),
            CancellationToken.None);

        Assert.Single(recorder.InterceptorCalls);
        Assert.Empty(recorder.Calls);
    }
}
