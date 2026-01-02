namespace DiscordEconomyBot.Models;

public class ReactionRole
{
    public ulong MessageId { get; set; }
    public ulong ChannelId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public ulong RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
