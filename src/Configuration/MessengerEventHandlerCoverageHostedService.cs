using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManagedDotNet.Messengers.Configuration;

/// <summary>
/// At host start, logs a warning for concrete <c>MessengerEvent</c> types that have no handler on a registered messenger.
/// Does not throw — uncovered events are skipped at dispatch time.
/// </summary>
internal sealed class MessengerEventHandlerCoverageHostedService : IHostedService
{
    private readonly MessengerEventHandlerRegistry _registry;
    private readonly ILogger<MessengerEventHandlerCoverageHostedService> _logger;

    public MessengerEventHandlerCoverageHostedService(
        MessengerEventHandlerRegistry registry,
        ILogger<MessengerEventHandlerCoverageHostedService> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach ((var messenger, IReadOnlyList<Type> uncovered) in _registry.GetUncoveredEvents())
        {
            _logger.LogWarning(
                "Messenger '{Messenger}' has no handler for event type(s): {EventTypes}. Updates of those types will be skipped.",
                messenger,
                string.Join(", ", uncovered.Select(t => t.Name)));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
