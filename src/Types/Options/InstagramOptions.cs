namespace ManagedDotNet.Messengers.Types.Options;

/// <summary>
/// Placeholder options for a future Instagram adapter.
/// </summary>
public sealed class InstagramOptions
{
    public const string SectionName = "InstagramOptions";

    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string RedirectUri { get; set; } = default!;
    public string[] Scopes { get; set; } = ["user_profile", "user_media"];
}
