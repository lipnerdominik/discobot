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
            await message.Channel.SendMessageAsync($"❌ Wystąpił błąd: {ex.Message}");
        }
    }

    private async Task HandleBalance(SocketMessage message, SocketGuildUser user)
    {
        var balance = _economyService.GetBalance(user.Id);
        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle("💰 Saldo")
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
            .WithTitle(success ? "🎁 Codzienna nagroda" : "⏰ Zbyt wcześnie")
            .WithDescription(msg)
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    private async Task HandleTop(SocketMessage message)
    {
        var topUsers = _economyService.GetTopUsers(10);

        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithTitle("🏆 Top 10 Najbogatszych")
            .WithDescription("Ranking użytkowników według salda monet")
            .WithCurrentTimestamp();

        for (int i = 0; i < topUsers.Count; i++)
        {
            var medal = i switch
            {
                0 => "🥇",
                1 => "🥈",
                2 => "🥉",
                _ => $"{i + 1}."
            };

            embed.AddField($"{medal} {topUsers[i].Username}",
                $"💰 {topUsers[i].Balance} monet",
                inline: false);
        }

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task HandleShop(SocketMessage message)
    {
        var roles = _roleShopService.GetAllRoles();

        if (!roles.Any())
        {
            await message.Channel.SendMessageAsync("🛒 Sklep jest obecnie pusty!");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("🛒 Sklep z Rangami")
            .WithDescription("Użyj `!kup <ID rangi>` aby kupić")
            .WithCurrentTimestamp();

        foreach (var role in roles)
        {
            var desc = string.IsNullOrEmpty(role.Description)
                ? $"💰 Cena: **{role.Price}** monet"
                : $"{role.Description}\n💰 Cena: **{role.Price}** monet";

            embed.AddField($"🎭 {role.RoleName}", desc, inline: false);
        }

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task HandleBuy(SocketMessage message, SocketGuildUser user, string[] parts)
    {
        if (parts.Length < 2)
        {
            await message.Channel.SendMessageAsync("❌ Użyj: `!kup <ID rangi>`");
            return;
        }

        if (!ulong.TryParse(parts[1], out var roleId))
        {
            await message.Channel.SendMessageAsync("❌ Nieprawidłowe ID rangi!");
            return;
        }

        var (success, msg) = await _roleShopService.BuyRole(user, roleId);
        var embed = new EmbedBuilder()
            .WithColor(success ? Color.Green : Color.Red)
            .WithTitle(success ? "✅ Zakup udany" : "❌ Błąd zakupu")
            .WithDescription(msg);

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task HandleStats(SocketMessage message, SocketGuildUser user)
    {
        var stats = _economyService.GetUserStats(user.Id);

        var embed = new EmbedBuilder()
            .WithColor(Color.Teal)
            .WithTitle($"📊 Statystyki - {user.Username}")
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .AddField("💰 Saldo", $"{stats.Balance} monet", inline: true)
            .AddField("📝 Wiadomości", stats.MessageCount.ToString(), inline: true)
            .AddField("🎤 Czas na voice", $"{stats.VoiceTime.TotalHours:F1}h", inline: true)
            .AddField("👥 Zaproszenia", stats.InviteCount.ToString(), inline: true)
            .AddField("📊 Ankiety", stats.PollParticipation.ToString(), inline: true)
            .AddField("🎉 Wydarzenia", stats.EventParticipation.ToString(), inline: true)
            .AddField("📅 Dni aktywności", stats.DaysActive.ToString(), inline: true)
            .AddField("🏆 Osiągnięcia", stats.Achievements.Count.ToString(), inline: true)
            .WithCurrentTimestamp()
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }

    private async Task HandleAchievements(SocketMessage message, SocketGuildUser user)
    {
        var stats = _economyService.GetUserStats(user.Id);

        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle($"🏆 Osiągnięcia - {user.Username}")
            .WithDescription(stats.Achievements.Any()
                ? string.Join("\n", stats.Achievements.Select(a => $"✅ {FormatAchievement(a)}"))
                : "Brak zdobytych osiągnięć")
            .WithCurrentTimestamp();

        await message.Channel.SendMessageAsync(embed: embed.Build());
    }

    private string FormatAchievement(string key)
    {
        return key switch
        {
            "100_messages" => "100 Wiadomości 📝",
            "500_messages" => "500 Wiadomości 📝✨",
            "1000_messages" => "1000 Wiadomości 📝🌟",
            "7_days_active" => "7 Dni Aktywności 🗓️",
            "30_days_active" => "30 Dni Aktywności 🗓️🌟",
            _ => key
        };
    }

    private async Task HandleHelp(SocketMessage message)
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle("📖 Pomoc - Komendy Ekonomii")
            .WithDescription("Lista dostępnych komend:")
            .AddField("💰 !saldo", "Sprawdź swoje saldo monet", inline: false)
            .AddField("🎁 !daily", "Odbierz codzienną nagrodę", inline: false)
            .AddField("🏆 !top", "Zobacz ranking najbogatszych", inline: false)
            .AddField("🛒 !sklep", "Zobacz dostępne rangi do kupienia", inline: false)
            .AddField("🎭 !kup <ID>", "Kup rangę ze sklepu", inline: false)
            .AddField("📊 !statystyki", "Zobacz swoje statystyki", inline: false)
            .AddField("🏆 !osiagniecia", "Zobacz swoje osiągnięcia", inline: false)
            .WithFooter("Zarabiaj monety pisząc wiadomości, spędzając czas na voice i biorąc udział w życiu serwera!")
            .WithCurrentTimestamp()
            .Build();

        await message.Channel.SendMessageAsync(embed: embed);
    }
}
