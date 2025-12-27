using Discord;
using Discord.WebSocket;
using DiscordEconomyBot.Data;
using DiscordEconomyBot.Models;

namespace DiscordEconomyBot.Services;

public class RoleShopService
{
    private readonly JsonDataStore _dataStore;
    private readonly EconomyService _economyService;

    public RoleShopService(JsonDataStore dataStore, EconomyService economyService)
    {
        _dataStore = dataStore;
        _economyService = economyService;
    }

    public List<ShopRole> GetAllRoles()
    {
        return _dataStore.GetAllRoles();
    }

    public void AddRole(ulong roleId, string roleName, long price, string description = "")
    {
        _dataStore.AddRole(new ShopRole
        {
            RoleId = roleId,
            RoleName = roleName,
            Price = price,
            Description = description
        });
    }

    public void RemoveRole(ulong roleId)
    {
        _dataStore.RemoveRole(roleId);
    }

    public async Task<(bool success, string message)> BuyRole(SocketGuildUser user, ulong roleId)
    {
        var role = _dataStore.GetAllRoles().FirstOrDefault(r => r.RoleId == roleId);
        if (role == null)
            return (false, "Ta ranga nie istnieje w sklepie!");

        if (user.Roles.Any(r => r.Id == roleId))
            return (false, "Już posiadasz tę rangę!");

        var balance = _economyService.GetBalance(user.Id);
        if (balance < role.Price)
            return (false, $"Nie masz wystarczająco monet! Potrzebujesz {role.Price}, a masz {balance}.");

        var guildRole = user.Guild.GetRole(roleId);
        if (guildRole == null)
            return (false, "Ta ranga nie istnieje na serwerze!");

        if (!_economyService.RemoveCoins(user.Id, role.Price, $"Zakup rangi: {role.RoleName}"))
            return (false, "Nie udało się pobrać monet!");

        try
        {
            await user.AddRoleAsync(guildRole);
            return (true, $"Pomyślnie zakupiono rangę **{role.RoleName}** za {role.Price} monet! 🎉");
        }
        catch (Exception ex)
        {
            // Zwróć monety jeśli przypisanie rangi się nie powiodło
            _economyService.AddCoins(user.Id, role.Price, "Zwrot za nieudany zakup rangi");
            return (false, $"Nie udało się przypisać rangi: {ex.Message}");
        }
    }
}
