using ManagedDotNet.Messengers.Types.Models;

namespace ManagedDotNet.Messengers.Types.Events;

public sealed class ButtonClicked : MessengerEvent
{
    /// <summary>
    /// Callback data of the clicked <see cref="Button"/>.
    /// </summary>
    public string? Metadata { get; init; }
}
