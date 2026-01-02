using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using DiscordEconomyBot.Bot;
using DiscordEconomyBot.Data;
using DiscordEconomyBot.Models;
using DiscordEconomyBot.Services;
using Discord.WebSocket;
using Discord.Interactions;

namespace DiscordEconomyBot;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Konfiguracja
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Discord Economy Bot Services
        var economyConfig = builder.Configuration.GetSection("EconomyConfig").Get<EconomyConfig>() ?? new EconomyConfig();
        builder.Services.AddSingleton(economyConfig);

        // Discord klient
        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = Discord.GatewayIntents.AllUnprivileged |
                           Discord.GatewayIntents.MessageContent |
                           Discord.GatewayIntents.GuildMembers |
                           Discord.GatewayIntents.GuildVoiceStates
        };
        builder.Services.AddSingleton(socketConfig);
        builder.Services.AddSingleton<DiscordSocketClient>();
        builder.Services.AddSingleton<InteractionService>(sp => 
            new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));

        // Serwisy
        builder.Services.AddSingleton<JsonDataStore>();
        builder.Services.AddSingleton<EconomyService>();
        builder.Services.AddSingleton<RoleShopService>();
        builder.Services.AddSingleton<VoiceTrackingService>();
        builder.Services.AddSingleton<MiningService>();
        builder.Services.AddSingleton<ShellGameService>();
        builder.Services.AddSingleton<ReactionRoleService>();

        // Komendy
        builder.Services.AddSingleton<Commands.EconomyCommands>();
        builder.Services.AddSingleton<Commands.AdminCommands>();

        // Bot
        builder.Services.AddSingleton<BotClient>();
        builder.Services.AddHostedService<BotHostedService>();

        var app = builder.Build();
        
        // Add shutdown event handler
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        
        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogInformation("Application is stopping...");
        });
        
        lifetime.ApplicationStopped.Register(() =>
        {
            logger.LogInformation("Application stopped");
        });

        // Health endpoint
        app.MapGet("/health", (BotClient botClient) =>
        {
            var health = botClient.GetHealthStatus();
            return Results.Ok(health);
        });

        // Root endpoint
        app.MapGet("/", () => Results.Ok(new
        {
            name = "Discord Economy Bot",
            version = VersionService.GetVersion(),
            commitHash = VersionService.GetCommitHash(),
            status = "Running",
            endpoints = new[]
            {
                "/health - Bot health and status information"
            }
        }));

        logger.LogInformation("Starting application...");
        await app.RunAsync();
        logger.LogInformation("Application has shut down");
    }
}
