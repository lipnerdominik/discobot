using DiscordEconomyBot.Services;

namespace DiscordEconomyBot.Models;

public class HealthStatus
{
    public string Status { get; set; } = "Healthy";
    public string Version { get; set; } = VersionService.GetVersion();
    public string CommitHash { get; set; } = VersionService.GetCommitHash();
    public string BotUsername { get; set; } = string.Empty;
    public int GuildCount { get; set; }
    public bool IsConnected { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Uptime => DateTime.UtcNow - StartTime;
}
