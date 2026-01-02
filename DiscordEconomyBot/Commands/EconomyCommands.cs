using Discord;
using Discord.WebSocket;
using DiscordEconomyBot.Services;

namespace DiscordEconomyBot.Commands;

public class EconomyCommands
{
    private readonly EconomyService _economyService;
    private readonly RoleShopService _roleShopService;

    public EconomyCommands(EconomyService economyService, RoleShopService roleShopService)
    {
        _economyService = economyService;
        _roleShopService = roleShopService;
    }

    public async Task HandleCommand(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        if (!message.Content.StartsWith("!")) return;

        var user = message.Author as SocketGuildUser;
        if (user == null) return;

        var parts = message.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLower();

        try
        {
            switch (command)
            {
                case "!saldo":
                case "!balance":
                    await HandleBalance(message, user);
                    break;

                case "!daily":
                    await HandleDaily(message, user);
                    break;

                case "!top":
                case "!topka":
                    await HandleTop(message);
                    break;

                case "!sklep":
                case "!shop":
                    await HandleShop(message);
                    break;

                case "!kup":
                case "!buy":
                    await HandleBuy(message, user, parts);
                    break;

                case "!statystyki":
                case "!stats":
                    await HandleStats(message, user);
                    break;

                case "!osiagniecia":
                case "!achievements":
                    await HandleAchievements(message, user);
                    break;

                case "!pomoc":
                case "!help":
                    await HandleHelp(message);
                    break;
            }
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($":x: Wystąpił błąd: {ex.Message}");
        }
    }

    private async Task HandleBalance(SocketMessage message, SocketGuildUser user)
    {
        var balance = _economyService.GetBalance(user.Id);
        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle(":moneybag: Saldo")
            .WithDescription($"{user.Mention}, masz **{balance}** monet!")
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .WithCurrentTimestamp()
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    private async Task HandleDaily(SocketMessage message, SocketGuildUser user)
    {
        var (success, msg) = _economyService.ClaimDaily(user.Id);
        var embed = new EmbedBuilder()
            .WithColor(success ? Color.Green : Color.Orange)
            .WithTitle(success ? ":gift: Codzienna nagroda" : ":clock: Zbyt wcześnie")
            .WithDescription(msg)
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    private async Task HandleTop(SocketMessage message)
    {
        var topUsers = _economyService.GetTopUsers(10);

        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithTitle(":trophy: Top 10 Najbogatszych")
            .WithDescription("Ranking użytkowników według salda monet")
            .WithCurrentTimestamp();

        for (int i = 0; i < topUsers.Count; i++)
        {
            var medal = i switch
            {
                0 => ":first_place:",
                1 => ":second_place:",
                2 => ":third_place:",
                _ => $"{i + 1}."
            };

            embed.AddField($"{medal} {topUsers[i].Username}",
                $":moneybag: {topUsers[i].Balance} monet",
                inline: false);
        }

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task HandleShop(SocketMessage message)
    {
        var roles = _roleShopService.GetAllRoles();

        if (!roles.Any())
        {
            await message.Channel.SendMessageAsync(":shopping_cart: Sklep jest obecnie pusty!");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle(":shopping_cart: Sklep z Rangami")
            .WithDescription("Użyj `!kup <ID rangi>` aby kupić")
            .WithCurrentTimestamp();

        foreach (var role in roles)
        {
            var desc = string.IsNullOrEmpty(role.Description)
                ? $":moneybag: Cena: **{role.Price}** monet"
                : $"{role.Description}\n:moneybag: Cena: **{role.Price}** monet";

            embed.AddField($":performing_arts: {role.RoleName}", desc, inline: false);
        }

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task HandleBuy(SocketMessage message, SocketGuildUser user, string[] parts)
    {
        if (parts.Length < 2)
        {
            await message.Channel.SendMessageAsync(":x: Użyj: `!kup <ID rangi>`");
            return;
        }

        if (!ulong.TryParse(parts[1], out var roleId))
        {
            await message.Channel.SendMessageAsync(":x: Nieprawidłowe ID rangi!");
            return;
        }

        var (success, msg) = await _roleShopService.BuyRole(user, roleId);
        var embed = new EmbedBuilder()
            .WithColor(success ? Color.Green : Color.Red)
            .WithTitle(success ? ":white_check_mark: Zakup udany" : ":x: Błąd zakupu")
            .WithDescription(msg);

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task HandleStats(SocketMessage message, SocketGuildUser user)
    {
        var stats = _economyService.GetUserStats(user.Id);

        var embed = new EmbedBuilder()
            .WithColor(Color.Teal)
            .WithTitle($":bar_chart: Statystyki - {user.Username}")
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .AddField(":moneybag: Saldo", $"{stats.Balance} monet", inline: true)
            .AddField(":pencil: Wiadomości", stats.MessageCount.ToString(), inline: true)
            .AddField(":microphone2: Czas na voice", $"{stats.VoiceTime.TotalHours:F1}h", inline: true)
            .AddField(":busts_in_silhouette: Zaproszenia", stats.InviteCount.ToString(), inline: true)
            .AddField(":bar_chart: Ankiety", stats.PollParticipation.ToString(), inline: true)
            .AddField(":tada: Wydarzenia", stats.EventParticipation.ToString(), inline: true)
            .AddField(":calendar: Dni aktywności", stats.DaysActive.ToString(), inline: true)
            .AddField(":trophy: Osiągnięcia", stats.Achievements.Count.ToString(), inline: true)
            .WithCurrentTimestamp()
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    private async Task HandleAchievements(SocketMessage message, SocketGuildUser user)
    {
        var stats = _economyService.GetUserStats(user.Id);

        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle($":trophy: Osiągnięcia - {user.Username}")
            .WithDescription(stats.Achievements.Any()
                ? string.Join("\n", stats.Achievements.Select(a => $":white_check_mark: {FormatAchievement(a)}"))
                : "Brak zdobytych osiągnięć")
            .WithCurrentTimestamp();

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private string FormatAchievement(string key)
    {
        return key switch
        {
            "100_messages" => "100 Wiadomości :pencil:",
            "500_messages" => "500 Wiadomości :pencil::sparkles:",
            "1000_messages" => "1000 Wiadomości :pencil::star2:",
            "7_days_active" => "7 Dni Aktywności :calendar:",
            "30_days_active" => "30 Dni Aktywności :calendar::star2:",
            _ => key
        };
    }

    private async Task HandleHelp(SocketMessage message)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle(":book: Pomoc - Komendy Ekonomii")
            .WithDescription("Lista dostępnych komend:")
            .AddField(":moneybag: !saldo", "Sprawdź swoje saldo monet", inline: false)
            .AddField(":gift: !daily", "Odbierz codzienną nagrodę", inline: false)
            .AddField(":trophy: !top", "Zobacz ranking najbogatszych", inline: false)
            .AddField(":shopping_cart: !sklep", "Zobacz dostępne rangi do kupienia", inline: false)
            .AddField(":performing_arts: !kup <ID>", "Kup rangę ze sklepu", inline: false)
            .AddField(":bar_chart: !statystyki", "Zobacz swoje statystyki", inline: false)
            .AddField(":trophy: !osiagniecia", "Zobacz swoje osiągnięcia", inline: false)
            .WithFooter("Zarabiaj monety pisząc wiadomości, spędzając czas na voice i biorąc udział w życiu serwera!")
            .WithCurrentTimestamp()
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }
}
