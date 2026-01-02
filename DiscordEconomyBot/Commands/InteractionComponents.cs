using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordEconomyBot.Services;

namespace DiscordEconomyBot.Commands;

public class InteractionComponents : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ShellGameService _shellGameService;
    private readonly EconomyService _economyService;

    public InteractionComponents(ShellGameService shellGameService, EconomyService economyService)
    {
        _shellGameService = shellGameService;
        _economyService = economyService;
    }

    [ComponentInteraction("shellgame_*_*")]
    public async Task HandleShellGameButton(int cupNumber, ulong userId)
    {
        // SprawdŸ czy to osoba, która rozpoczê³a grê
        if (Context.User.Id != userId)
        {
            await RespondAsync(":x: To nie Twoja gra! Rozpocznij w³asn¹ u¿ywaj¹c `/kubki`", ephemeral: true);
            return;
        }

        var (exists, won, message, winAmount) = _shellGameService.GuessAndEnd(Context.User.Id, cupNumber);

        if (!exists)
        {
            await RespondAsync($":x: {message}", ephemeral: true);
            return;
        }

        // Usuñ buttony z oryginalnej wiadomoœci
        var originalMessage = (Context.Interaction as SocketMessageComponent)?.Message;
        if (originalMessage != null)
        {
            await originalMessage.ModifyAsync(msg =>
            {
                msg.Components = new ComponentBuilder().Build();
            });
        }

        var embed = new EmbedBuilder()
            .WithColor(won ? Color.Green : Color.Red)
            .WithTitle(won ? ":trophy: WYGRANA!" : ":x: PRZEGRANA!")
            .WithDescription($"{message}")
            .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithFooter(won ? "Gratulacje! Zagraj ponownie u¿ywaj¹c /kubki" : "Spróbuj jeszcze raz! U¿yj /kubki")
            .WithCurrentTimestamp();

        if (won)
        {
            embed.AddField(":money_with_wings: Twoja wygrana", $"**{winAmount}** monet", inline: true)
                .AddField(":chart_with_upwards_trend: Nowe saldo", $"**{_economyService.GetBalance(Context.User.Id)}** monet", inline: true);
        }
        else
        {
            embed.AddField(":chart_with_downwards_trend: Nowe saldo", $"**{_economyService.GetBalance(Context.User.Id)}** monet", inline: true);
        }

        await RespondAsync(embed: embed.Build());
    }
}
