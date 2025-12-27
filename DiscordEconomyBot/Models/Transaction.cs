namespace DiscordEconomyBot.Models;

public class Transaction
{
    public ulong UserId { get; set; }
    public long Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "earn", "spend", "admin_add", "admin_remove"
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
