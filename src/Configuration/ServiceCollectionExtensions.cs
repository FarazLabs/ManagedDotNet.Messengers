using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Core;
using ManagedDotNet.Messengers.Core.Telegram;
using ManagedDotNet.Messengers.Core.Telegram.Polling;
using ManagedDotNet.Messengers.Types.Enums;
using ManagedDotNet.Messengers.Types.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace ManagedDotNet.Messengers.Configuration;

/// <summary>
/// Extension methods for registering messenger services.
/// </summary>
public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Registers the Telegram <see cref="IMessengerTransport"/>, <see cref="IMessengerBroker"/>, and long-polling
    /// that dispatches inbound messages to handlers registered via
    /// <see cref="MessengerConfigurationBuilder.WithEventHandler{TEvent, THandler}"/>.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configure">Sets <see cref="TelegramOptions"/> (e.g. <c>BotToken</c>)</param>
    public static MessengerConfigurationBuilder AddTelegram(this IServiceCollection services, Action<TelegramOptions> configure)
    {
        services.Configure(configure);
        return AddTelegramCore(services);
    }

    /// <inheritdoc cref="AddTelegram(IServiceCollection, Action{TelegramOptions})"/>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Root configuration; binds the <c>TelegramOptions</c> section</param>
    public static MessengerConfigurationBuilder AddTelegram(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        return AddTelegramCore(services);
    }

    private static MessengerConfigurationBuilder AddTelegramCore(IServiceCollection services)
    {
        services.TryAddScoped<IMessengerBroker, MessengerBroker>();
        services.TryAddSingleton<IMessengerEventDispatcher, MessengerEventDispatcher>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, MessengerEventHandlerCoverageHostedService>());

        var registry = MessengerEventHandlerRegistry.GetOrCreate(services);

        registry.AddMessenger(Messenger.Telegram);

        services.AddTransient<IMessengerTransport, TelegramMessengerGateway>();

        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfiguration = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                ArgumentException.ThrowIfNullOrWhiteSpace(botConfiguration.BotToken);
                var options = new TelegramBotClientOptions(botConfiguration.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddScoped<TelegramUpdateHandler>();
        services.AddScoped<TelegramReceiverService>();
        services.AddHostedService<TelegramPollingService>();

        return new MessengerConfigurationBuilder(services, Messenger.Telegram, registry);
    }

}
