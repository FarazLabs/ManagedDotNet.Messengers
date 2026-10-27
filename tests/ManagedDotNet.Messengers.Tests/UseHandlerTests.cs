using ManagedDotNet.Messengers.Configuration;
using ManagedDotNet.Messengers.Tests.Fixtures;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ManagedDotNet.Messengers.Tests;

public class UseHandlerTests
{
    private static MessengerConfigurationBuilder AddTelegram(IServiceCollection services) =>
        services.AddTelegram(o => o.BotToken = "test-token");

    [Fact]
    public void WithEventHandler_registers_binding_and_scoped_handler()
    {
        var services = new ServiceCollection();

        AddTelegram(services)
            .WithEventHandler<TextMessageReceived, TextHandler>();

        var registry = MessengerEventHandlerRegistry.GetOrCreate(services);
        Assert.True(registry.TryGetBinding(Messenger.Telegram, typeof(TextMessageReceived), out var binding));
        Assert.Equal(typeof(TextHandler), binding.HandlerType);
        Assert.Contains(services, d => d.ServiceType == typeof(TextHandler) && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void WithEventHandler_can_bind_same_handler_to_multiple_events()
    {
        var services = new ServiceCollection();

        AddTelegram(services)
            .WithEventHandler<VoiceMessageReceived, MediaHandler>()
            .WithEventHandler<ButtonClicked, MediaHandler>();

        var registry = MessengerEventHandlerRegistry.GetOrCreate(services);
        Assert.True(registry.TryGetBinding(Messenger.Telegram, typeof(VoiceMessageReceived), out var voice));
        Assert.True(registry.TryGetBinding(Messenger.Telegram, typeof(ButtonClicked), out var click));
        Assert.Equal(typeof(MediaHandler), voice.HandlerType);
        Assert.Equal(typeof(MediaHandler), click.HandlerType);
        Assert.False(registry.TryGetBinding(Messenger.Telegram, typeof(TextMessageReceived), out _));
        Assert.Single(services, d => d.ServiceType == typeof(MediaHandler));
    }

    [Fact]
    public void Duplicate_event_type_on_same_messenger_throws()
    {
        var builder = AddTelegram(new ServiceCollection())
            .WithEventHandler<TextMessageReceived, TextHandler>();

        var ex = Assert.Throws<MisconfiguredException>(() =>
            builder.WithEventHandler<TextMessageReceived, OtherTextHandler>());
        Assert.Contains(nameof(TextMessageReceived), ex.Message);
    }

    [Fact]
    public void Adding_same_messenger_twice_throws()
    {
        var services = new ServiceCollection();
        AddTelegram(services);

        Assert.Throws<MisconfiguredException>(() => AddTelegram(services));
    }

    [Fact]
    public async Task Uncovered_events_log_warning_on_host_start_without_throwing()
    {
        var warnings = new List<string>();
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(new CollectingLoggerProvider(warnings)));
        services.AddSingleton(new Recorder());

        AddTelegram(services)
            .WithEventHandler<TextMessageReceived, TextHandler>();

        await using ServiceProvider sp = services.BuildServiceProvider();
        IHostedService coverage = sp.GetServices<IHostedService>()
            .OfType<MessengerEventHandlerCoverageHostedService>()
            .Single();

        Exception? ex = await Record.ExceptionAsync(() => coverage.StartAsync(CancellationToken.None));
        Assert.Null(ex);

        string warning = Assert.Single(warnings);
        Assert.Contains("Telegram", warning);
        Assert.Contains(nameof(VoiceMessageReceived), warning);
        Assert.Contains(nameof(ButtonClicked), warning);
        Assert.Contains(nameof(TextMessageEdited), warning);
        Assert.DoesNotContain(nameof(TextMessageReceived), warning);
    }

    private sealed class CollectingLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _warnings;

        public CollectingLoggerProvider(List<string> warnings) => _warnings = warnings;

        public ILogger CreateLogger(string categoryName) => new CollectingLogger(_warnings);

        public void Dispose() { }

        private sealed class CollectingLogger(List<string> warnings) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (logLevel >= LogLevel.Warning)
                    warnings.Add(formatter(state, exception));
            }
        }
    }
}
