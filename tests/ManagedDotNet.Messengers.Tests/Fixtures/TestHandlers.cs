using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Tests.Fixtures;

public sealed class Recorder
{
    public List<(MessengerEvent Message, EventEnvelope Envelope, Type Handler)> Calls { get; } = new();

    public List<(string Name, EventEnvelope Envelope)> InterceptorCalls { get; } = new();
}

public sealed class TextHandler(Recorder recorder) : IMessengerMessageHandler<TextMessageReceived>
{
    public Task Handle(EventEnvelope<TextMessageReceived> envelope, CancellationToken cancellationToken)
    {
        recorder.Calls.Add((envelope.Event, envelope, typeof(TextHandler)));
        return Task.CompletedTask;
    }
}

public sealed class OtherTextHandler : IMessengerMessageHandler<TextMessageReceived>
{
    public Task Handle(EventEnvelope<TextMessageReceived> envelope, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

public sealed class MediaHandler(Recorder recorder) :
    IMessengerMessageHandler<VoiceMessageReceived>,
    IMessengerMessageHandler<ButtonClicked>
{
    public Task Handle(EventEnvelope<VoiceMessageReceived> envelope, CancellationToken cancellationToken)
    {
        recorder.Calls.Add((envelope.Event, envelope, typeof(MediaHandler)));
        return Task.CompletedTask;
    }

    public Task Handle(EventEnvelope<ButtonClicked> envelope, CancellationToken cancellationToken)
    {
        recorder.Calls.Add((envelope.Event, envelope, typeof(MediaHandler)));
        return Task.CompletedTask;
    }
}

public sealed class FirstInterceptor(Recorder recorder) : IMessengerEventInterceptor
{
    public async Task Intercept(EventEnvelope envelope, Func<Task> next, CancellationToken cancellationToken)
    {
        recorder.InterceptorCalls.Add(("first", envelope));
        await next();
    }
}

public sealed class SecondInterceptor(Recorder recorder) : IMessengerEventInterceptor
{
    public async Task Intercept(EventEnvelope envelope, Func<Task> next, CancellationToken cancellationToken)
    {
        recorder.InterceptorCalls.Add(("second", envelope));
        await next();
    }
}

public sealed class ShortCircuitInterceptor(Recorder recorder) : IMessengerEventInterceptor
{
    public Task Intercept(EventEnvelope envelope, Func<Task> next, CancellationToken cancellationToken)
    {
        recorder.InterceptorCalls.Add(("short-circuit", envelope));
        return Task.CompletedTask;
    }
}
