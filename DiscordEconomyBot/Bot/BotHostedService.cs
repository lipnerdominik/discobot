using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DiscordEconomyBot.Services;

namespace DiscordEconomyBot.Bot;

public class BotHostedService : IHostedService
{
    private readonly BotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<BotHostedService> _logger;
    private Task? _botTask;
    private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

    public BotHostedService(
        BotClient botClient, 
        IConfiguration configuration, 
        IHostEnvironment environment,
        ILogger<BotHostedService> logger)
    {
        _botClient = botClient;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discord Economy Bot - Uruchamianie...");
        _logger.LogInformation("Version: {Version}", VersionService.GetFullVersionInfo());
        _logger.LogInformation("Environment: {Environment}", _environment.EnvironmentName);
        
        var botToken = _configuration["BotToken"];
        _logger.LogInformation("Token z konfiguracji: {TokenStatus}", 
            string.IsNullOrEmpty(botToken) ? "BRAK" : "OK");
        
        if (string.IsNullOrEmpty(botToken) || botToken == "YOUR_BOT_TOKEN_HERE")
        {
            _logger.LogError("Błąd: Nie ustawiono tokena bota w appsettings.json!");
            throw new InvalidOperationException("Bot token not configured.");
        }

        _logger.LogInformation("Uwaga: aby slash-komendy działały natychmiast, zaproś bota ze scope `applications.commands` i sprawdź uprawnienia.");
        
        // Start bot in background task
        _botTask = _botClient.StartAsync(botToken, _stoppingCts.Token);
        
        // Wait a moment to ensure bot started successfully
        await Task.Delay(1000, cancellationToken);
        
        _logger.LogInformation("Bot started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Zatrzymywanie bota...");
        
        try
        {
            // Signal cancellation
            _stoppingCts.Cancel();
            
            // Stop the bot
            await _botClient.StopAsync();
            
            // Wait for the bot task to complete (with timeout)
            if (_botTask != null)
            {
                await Task.WhenAny(_botTask, Task.Delay(5000, cancellationToken));
            }
            
            _logger.LogInformation("Bot zatrzymany pomyślnie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zatrzymywania bota");
        }
    }

    public void Dispose()
    {
        _stoppingCts?.Dispose();
    }
}
