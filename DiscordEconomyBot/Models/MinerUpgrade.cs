namespace DiscordEconomyBot.Models;

public class MinerUpgrade
{
    public int Level { get; set; }
    public long Cost { get; set; }
    public double DoubleDropChance { get; set; } // Szansa w procentach
    public string Description { get; set; } = string.Empty;
}
