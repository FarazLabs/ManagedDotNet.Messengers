namespace ManagedDotNet.Messengers.Types.Models;

/// <summary>
/// Inline / reply button attached to an outbound messenger message.
/// </summary>
public class Button
{
    public ButtonType Type { get; set; }
    public string Text { get; set; } = default!;

    /// <summary>
    /// For <see cref="ButtonType.CallBackQuery"/>: callback data.
    /// For <see cref="ButtonType.Url"/>: web-app / URL metadata.
    /// </summary>
    public string Metadata { get; set; } = default!;
}
public enum ButtonType
{
    Url,
    CallBackQuery,
}
