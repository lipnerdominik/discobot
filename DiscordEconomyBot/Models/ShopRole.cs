namespace DiscordEconomyBot.Models;

public class ShopRole
{
    public ulong RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public long Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
