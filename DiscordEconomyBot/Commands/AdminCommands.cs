using Discord;
using Discord.WebSocket;
using DiscordEconomyBot.Services;

namespace DiscordEconomyBot.Commands;

public class AdminCommands
{
    private readonly EconomyService _economyService;
    private readonly RoleShopService _roleShopService;

    public AdminCommands(EconomyService economyService, RoleShopService roleShopService)
    {
        _economyService = economyService;
        _roleShopService = roleShopService;
    }

    public async Task HandleCommand(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        if (!message.Content.StartsWith("!admin")) return;

        var user = message.Author as SocketGuildUser;
        if (user == null) return;

        // Sprawdź czy użytkownik ma uprawnienia administratora
        if (!user.GuildPermissions.Administrator)
        {
            await message.Channel.SendMessageAsync("❌ Nie masz uprawnień do wykonania tej komendy!");
            return;
        }

        var parts = message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return;

        var subCommand = parts[1].ToLower();

        try
        {
            switch (subCommand)
            {
                case "dodajmonety":
                case "addcoins":
                    await HandleAddCoins(message, user, parts);
                    break;

                case "usunmonety":
                case "removecoins":
                    await HandleRemoveCoins(message, user, parts);
                    break;

                case "dodajrange":
                case "addrole":
                    await HandleAddRole(message, parts);
                    break;

                case "usunrange":
                case "removerole":
                    await HandleRemoveRole(message, parts);
                    break;

                case "wydarzenie":
                case "event":
                    await HandleEvent(message, parts);
                    break;

                case "pomoc":
                case "help":
                    await HandleAdminHelp(message);
                    break;
            }
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"❌ Wystąpił błąd: {ex.Message}");
        }
    }

    private async Task HandleAddCoins(SocketMessage message, SocketGuildUser admin, string[] parts)
    {
        if (parts.Length < 4)
        {
            await message.Channel.SendMessageAsync("❌ Użyj: `!admin dodajmonety <@użytkownik> <ilość>`");
            return;
        }

        if (message.MentionedUsers.Count == 0)
        {
            await message.Channel.SendMessageAsync("❌ Musisz oznaczyć użytkownika!");
            return;
        }

        if (!long.TryParse(parts[3], out var amount) || amount <= 0)
        {
            await message.Channel.SendMessageAsync("❌ Nieprawidłowa ilość monet!");
            return;
        }

        var targetUser = message.MentionedUsers.First();
        _economyService.AddCoins(targetUser.Id, amount, $"Admin {admin.Username} dodał monety");

        await message.Channel.SendMessageAsync($"✅ Dodano **{amount}** monet dla {targetUser.Mention}");
    }

    private async Task HandleRemoveCoins(SocketMessage message, SocketGuildUser admin, string[] parts)
    {
        if (parts.Length < 4)
        {
            await message.Channel.SendMessageAsync("❌ Użyj: `!admin usunmonety <@użytkownik> <ilość>`");
            return;
        }

        if (message.MentionedUsers.Count == 0)
        {
            await message.Channel.SendMessageAsync("❌ Musisz oznaczyć użytkownika!");
            return;
        }

        if (!long.TryParse(parts[3], out var amount) || amount <= 0)
        {
            await message.Channel.SendMessageAsync("❌ Nieprawidłowa ilość monet!");
            return;
        }

        var targetUser = message.MentionedUsers.First();
        var success = _economyService.RemoveCoins(targetUser.Id, amount, $"Admin {admin.Username} usunął monety");

        if (success)
            await message.Channel.SendMessageAsync($"✅ Usunięto **{amount}** monet od {targetUser.Mention}");
        else
            await message.Channel.SendMessageAsync($"❌ Użytkownik nie ma wystarczająco monet!");
    }

    private async Task HandleAddRole(SocketMessage message, string[] parts)
    {
        if (parts.Length < 5)
        {
            await message.Channel.SendMessageAsync("❌ Użyj: `!admin dodajrange <ID_rangi> <cena> <opis>`");
            return;
        }

        if (!ulong.TryParse(parts[2], out var roleId))
        {
            await message.Channel.SendMessageAsync("❌ Nieprawidłowe ID rangi!");
            return;
        }

        if (!long.TryParse(parts[3], out var price) || price <= 0)
        {
            await message.Channel.SendMessageAsync("❌ Nieprawidłowa cena!");
            return;
        }

        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        var role = guild?.GetRole(roleId);

        if (role == null)
        {
            await message.Channel.SendMessageAsync("❌ Nie znaleziono rangi o tym ID!");
            return;
        }

        var description = string.Join(" ", parts.Skip(4));
        _roleShopService.AddRole(roleId, role.Name, price, description);

        await message.Channel.SendMessageAsync($"✅ Dodano rangę **{role.Name}** do sklepu za **{price}** monet!");
    }

    private async Task HandleRemoveRole(SocketMessage message, string[] parts)
    {
        if (parts.Length < 3)
        {
            await message.Channel.SendMessageAsync("❌ Użyj: `!admin usunrange <ID_rangi>`");
            return;
        }

        if (!ulong.TryParse(parts[2], out var roleId))
        {
            await message.Channel.SendMessageAsync("❌ Nieprawidłowe ID rangi!");
            return;
        }

        _roleShopService.RemoveRole(roleId);
        await message.Channel.SendMessageAsync($"✅ Usunięto rangę ze sklepu!");
    }

    private async Task HandleEvent(SocketMessage message, string[] parts)
    {
        if (parts.Length < 3)
        {
            await message.Channel.SendMessageAsync("❌ Użyj: `!admin wydarzenie <@użytkownicy...>`");
            return;
        }

        if (message.MentionedUsers.Count == 0)
        {
            await message.Channel.SendMessageAsync("❌ Musisz oznaczyć przynajmniej jednego użytkownika!");
            return;
        }

        foreach (var user in message.MentionedUsers)
        {
            _economyService.HandleEventParticipation(user.Id);
        }

        await message.Channel.SendMessageAsync($"✅ Przyznano nagrody za wydarzenie dla {message.MentionedUsers.Count} użytkowników! 🎉");
    }

    private async Task HandleAdminHelp(SocketMessage message)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Red)
            .WithTitle("🔧 Pomoc - Komendy Administratora")
            .WithDescription("Lista dostępnych komend administracyjnych:")
            .AddField("💰 !admin dodajmonety <@użytkownik> <ilość>", "Dodaj monety użytkownikowi", inline: false)
            .AddField("💸 !admin usunmonety <@użytkownik> <ilość>", "Usuń monety użytkownikowi", inline: false)
            .AddField("🎭 !admin dodajrange <ID> <cena> <opis>", "Dodaj rangę do sklepu", inline: false)
            .AddField("🗑️ !admin usunrange <ID>", "Usuń rangę ze sklepu", inline: false)
            .AddField("🎉 !admin wydarzenie <@użytkownicy>", "Przyznaj nagrody za wydarzenie", inline: false)
            .WithFooter("Tylko administratorzy mogą używać tych komend")
            .WithCurrentTimestamp()
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }
}
