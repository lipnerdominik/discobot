namespace DiscordEconomyBot.Models;

public class UserBalance
{
    public ulong UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public long Balance { get; set; }
    public DateTime LastDaily { get; set; }
    public int MessageCount { get; set; }
    public TimeSpan VoiceTime { get; set; }
    public int InviteCount { get; set; }
    public int PollParticipation { get; set; }
    public int EventParticipation { get; set; }
    public int DaysActive { get; set; }
    public DateTime LastActivity { get; set; }
    public List<string> Achievements { get; set; } = new();
}
