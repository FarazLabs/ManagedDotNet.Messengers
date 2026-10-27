namespace ManagedDotNet.Messengers.Types.Models;

/// <summary>
/// Sender profile from the messenger platform.
/// </summary>
public class UserData
{
    public string AccountId { get; init; } = default!;
    public string? UserName { get; init; }
    public string? Name { get; init; }
    public string? Picture { get; init; }
    public string? Language { get; init; }
}
