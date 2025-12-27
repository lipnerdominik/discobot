namespace DiscordEconomyBot.Models;

public class EconomyConfig
{
    public int CoinsPerMessage { get; set; } = 5;
    public int MessageCooldownSeconds { get; set; } = 30;
    public int CoinsPerVoiceMinute { get; set; } = 2;
    public int CoinsPerInvite { get; set; } = 100;
    public int CoinsPerPoll { get; set; } = 10;
    public int CoinsPerEvent { get; set; } = 50;
    public int DailyReward { get; set; } = 100;
    public Dictionary<string, int> Achievements { get; set; } = new()
    {
        ["100_messages"] = 50,
        ["500_messages"] = 200,
        ["1000_messages"] = 500,
        ["7_days_active"] = 150,
        ["30_days_active"] = 1000
    };
}
