using Discord.Interactions;
using Discord;
using DiscordEconomyBot.Services;
using DiscordEconomyBot.Models;

namespace DiscordEconomyBot.Commands;

public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly EconomyService _economyService;

    public SlashCommands(EconomyService economyService)
    {
        _economyService = economyService;
    }

    [SlashCommand("saldo", "SprawdŸ swoje saldo monet")]
    public async Task Saldo()
    {
        var userId = Context.User.Id;
        var stats = _economyService.GetUserStats(userId);
        var embed = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithTitle($"Saldo {Context.User.Username}")
            .WithDescription($"Monety: **{stats.Balance}**")
            .Build();
        await RespondAsync(embed: embed, ephemeral: true);
    }

    [SlashCommand("daily", "Odbierz codzienn¹ nagrodê")]
    public async Task Daily()
    {
        var result = _economyService.ClaimDaily(Context.User.Id);
        await RespondAsync(result.success ? "? Otrzymano codzienn¹ nagrodê!" : $"? {result.message}", ephemeral: true);
    }

    [SlashCommand("top", "Zobacz ranking najbogatszych")]
    public async Task Top()
    {
        var top = _economyService.GetTopUsers(10);
        var desc = string.Join("\n", top.Select((u, i) => $"{i + 1}. <@{u.UserId}> — **{u.Balance}**"));
        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle("?? Top 10")
            .WithDescription(desc)
            .Build();
        await RespondAsync(embed: embed);
    }
}
