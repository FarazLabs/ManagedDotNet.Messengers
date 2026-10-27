using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Abstractions;

/// <summary>
/// Messenger-scoped middleware for inbound events. Not bound to a specific event type.
/// Register with <c>UseInterceptor&lt;TInterceptor&gt;()</c>; runs in registration order before the handler.
/// </summary>
public interface IMessengerEventInterceptor
{
    /// <param name="envelope">The same envelope instance the typed handler will receive.</param>
    /// <param name="next">Invokes the rest of the pipeline (remaining interceptors, then the handler).</param>
    /// <param name="cancellationToken">A token that signals cancellation, typically during host shutdown.</param>
    Task Intercept(EventEnvelope envelope, Func<Task> next, CancellationToken cancellationToken);
}
