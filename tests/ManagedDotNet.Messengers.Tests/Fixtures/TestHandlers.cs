using ManagedDotNet.Messengers.Abstractions;
using ManagedDotNet.Messengers.Types.Events;
using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Tests.Fixtures;

public sealed class Recorder
{
    public List<(MessengerEvent Message, EventEnvelope<MessengerEvent> Envelope, Type Handler)> Calls { get; } = new();
}

public sealed class TextHandler(Recorder recorder) : IMessengerMessageHandler<TextMessageReceived>
{
    public Task Handle(EventEnvelope<TextMessageReceived> envelope, CancellationToken cancellationToken)
    {
        recorder.Calls.Add((envelope.Event, AsBase(envelope), typeof(TextHandler)));
        return Task.CompletedTask;
    }

    private static EventEnvelope<MessengerEvent> AsBase(EventEnvelope<TextMessageReceived> envelope) =>
        new()
        {
            Event = envelope.Event,
            User = envelope.User,
            Chat = envelope.Chat,
            CorrelationId = envelope.CorrelationId,
        };
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
        recorder.Calls.Add((envelope.Event, AsBase(envelope), typeof(MediaHandler)));
        return Task.CompletedTask;
    }

    public Task Handle(EventEnvelope<ButtonClicked> envelope, CancellationToken cancellationToken)
    {
        recorder.Calls.Add((envelope.Event, AsBase(envelope), typeof(MediaHandler)));
        return Task.CompletedTask;
    }

    private static EventEnvelope<MessengerEvent> AsBase<T>(EventEnvelope<T> envelope) where T : MessengerEvent =>
        new()
        {
            Event = envelope.Event,
            User = envelope.User,
            Chat = envelope.Chat,
            CorrelationId = envelope.CorrelationId,
        };
}
