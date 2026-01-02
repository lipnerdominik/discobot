namespace DiscordEconomyBot.Models;

public class UserInventory
{
    public ulong UserId { get; set; }
    public Dictionary<string, int> Items { get; set; } = new();
    public DateTime LastMining { get; set; }
    public int TotalMiningCount { get; set; }
    public int MinerLevel { get; set; } = 0; // Poziom ulepszenia górnika
}
