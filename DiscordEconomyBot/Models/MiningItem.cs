namespace DiscordEconomyBot.Models;

public class MiningItem
{
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public long Value { get; set; }
    public double DropChance { get; set; }
    public Rarity Rarity { get; set; }
}

public enum Rarity
{
    Common,      // Pospolite
    Uncommon,    // Rzadkie
    Rare,        // Bardzo rzadkie
    Epic,        // Epickie
    Legendary    // Legendarne
}
