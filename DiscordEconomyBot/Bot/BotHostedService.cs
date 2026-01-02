using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordEconomyBot.Bot;

public class BotHostedService : IHostedService
{
    private readonly BotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<BotHostedService> _logger;

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
        _logger.LogInformation("?? Discord Economy Bot - Uruchamianie...");
        _logger.LogInformation("Environment: {Environment}", _environment.EnvironmentName);
        
        var botToken = _configuration["BotToken"];
        _logger.LogInformation("Token z konfiguracji: {TokenStatus}", 
            string.IsNullOrEmpty(botToken) ? "BRAK" : "OK");
        
        if (string.IsNullOrEmpty(botToken) || botToken == "YOUR_BOT_TOKEN_HERE")
        {
            _logger.LogError("? B≥πd: Nie ustawiono tokena bota w appsettings.json!");
            throw new InvalidOperationException("Bot token not configured.");
        }

        _logger.LogInformation("Uwaga: aby slash-komendy dzia≥a≥y natychmiast, zaproú bota ze scope `applications.commands` i sprawdü uprawnienia.");
        await _botClient.StartAsync(botToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Zatrzymywanie bota...");
        await _botClient.StopAsync();
    }
}
