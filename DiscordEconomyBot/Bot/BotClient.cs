using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using DiscordEconomyBot.Commands;
using DiscordEconomyBot.Services;
using Discord.Interactions;

namespace DiscordEconomyBot.Bot;

public class BotClient
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly EconomyService _economyService;
    private readonly VoiceTrackingService _voiceTrackingService;
    private readonly EconomyCommands _economyCommands;
    private readonly AdminCommands _adminCommands;
    private readonly ILogger<BotClient> _logger;

    public BotClient(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services,
        EconomyService economyService,
        RoleShopService roleShopService,
        VoiceTrackingService voiceTrackingService,
        EconomyCommands economyCommands,
        AdminCommands adminCommands,
        ILogger<BotClient> logger)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
        _economyService = economyService;
        _voiceTrackingService = voiceTrackingService;
        _economyCommands = economyCommands;
        _adminCommands = adminCommands;
        _logger = logger;

        _interactions.Log += LogAsync;

        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdatedAsync;
        _client.UserJoined += UserJoinedAsync;
        _client.InteractionCreated += HandleInteractionAsync;
    }

    private Task LogAsync(LogMessage log)
    {
        var logLevel = log.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, log.Exception, "[{Source}] {Message}", log.Source, log.Message);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        _logger.LogInformation("Bot zalogowany jako {Username}#{Discriminator}", 
            _client.CurrentUser.Username, _client.CurrentUser.Discriminator);

        // Rejestruj moduły slash-komend z DI
        await _interactions.AddModulesAsync(typeof(SlashCommands).Assembly, _services);

        // Publikuj komendy jako guildowe (natychmiastowe) dla wszystkich gildii, w których jest bot
        foreach (var guild in _client.Guilds)
        {
            await _interactions.RegisterCommandsToGuildAsync(guild.Id);
        }
        
        _logger.LogInformation("Zarejestrowano slash-komendy dla {GuildCount} gildii", _client.Guilds.Count);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactions.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wykonywania interakcji");
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                try { await interaction.GetOriginalResponseAsync(); }
                catch { /* ignore */ }
            }
        }
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        var user = message.Author as SocketGuildUser;
        if (user == null) return;

        // Odpowiedź na wzmiankę bota
        if (message.MentionedUsers.Any(u => u.Id == _client.CurrentUser.Id))
        {
            // Dodaj reakcję i odpowiedź tekstową
            try
            {
                await message.AddReactionAsync(new Emoji("👋"));
            }
            catch { /* ignore reaction failures */ }

            await message.Channel.SendMessageAsync($"Cześć, {user.Mention}! 👋");
            return; // nie przetwarzaj dalej jako komendę
        }

        // Obsługa komend tekstowych
        if (message.Content.StartsWith("!"))
        {
            if (message.Content.StartsWith("!admin"))
            {
                await _adminCommands.HandleCommand(message);
            }
            else
            {
                await _economyCommands.HandleCommand(message);
            }
        }
        else
        {
            // Nagroda za wiadomość
            _economyService.HandleMessageSent(user.Id, user.Username);
        }
    }

    private Task UserVoiceStateUpdatedAsync(SocketUser socketUser, SocketVoiceState before, SocketVoiceState after)
    {
        var user = socketUser as SocketGuildUser;
        if (user == null || user.IsBot) return Task.CompletedTask;

        _voiceTrackingService.UserMovedVoice(user.Id, before.VoiceChannel, after.VoiceChannel);

        return Task.CompletedTask;
    }

    private Task UserJoinedAsync(SocketGuildUser user)
    {
        _logger.LogInformation("Nowy użytkownik dołączył: {Username}", user.Username);
        return Task.CompletedTask;
    }

    public async Task StartAsync(string token)
    {
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Czekaj na zakończenie (zarządzane przez host)
        await Task.Delay(Timeout.Infinite);
    }

    public async Task StopAsync()
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
    }
}
