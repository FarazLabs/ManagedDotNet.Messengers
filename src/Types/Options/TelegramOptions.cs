namespace ManagedDotNet.Messengers.Types.Options;

public sealed class TelegramOptions
{
    public const string SectionName = "TelegramOptions";

    public string BotToken { get; set; } = default!;
}
