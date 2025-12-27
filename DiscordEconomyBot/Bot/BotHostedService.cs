using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace DiscordEconomyBot.Bot;

public class BotHostedService : IHostedService
{
    private readonly BotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public BotHostedService(BotClient botClient, IConfiguration configuration, IHostEnvironment environment)
    {
        _botClient = botClient;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Environment: {_environment.EnvironmentName}");
        
        var botToken = _configuration["BotToken"];
        Console.WriteLine($"Token z konfiguracji: {(string.IsNullOrEmpty(botToken) ? "BRAK" : "OK")}");
        
        if (string.IsNullOrEmpty(botToken) || botToken == "YOUR_BOT_TOKEN_HERE")
        {
            Console.WriteLine("? B≥πd: Nie ustawiono tokena bota w appsettings.json!");
            throw new InvalidOperationException("Bot token not configured.");
        }

        Console.WriteLine("Uwaga: aby slash-komendy dzia≥a≥y natychmiast, zaproú bota ze scope `applications.commands` i sprawdü uprawnienia.");
        await _botClient.StartAsync(botToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _botClient.StopAsync();
    }
}
