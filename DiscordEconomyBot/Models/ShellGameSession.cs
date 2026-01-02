namespace DiscordEconomyBot.Models;

public class ShellGameSession
{
    public ulong UserId { get; set; }
    public int CorrectCup { get; set; }
    public long BetAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
