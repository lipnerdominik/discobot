using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        Console.WriteLine("🤖 Discord Economy Bot - Uruchamianie...\n");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Konfiguracja
                var economyConfig = configuration.GetSection("EconomyConfig").Get<EconomyConfig>() ?? new EconomyConfig();
                services.AddSingleton(economyConfig);

                // Discord klient
                var socketConfig = new DiscordSocketConfig
                {
                    GatewayIntents = Discord.GatewayIntents.AllUnprivileged |
                                   Discord.GatewayIntents.MessageContent |
                                   Discord.GatewayIntents.GuildMembers |
                                   Discord.GatewayIntents.GuildVoiceStates
                };
                services.AddSingleton(socketConfig);
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<InteractionService>(sp => 
                    new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));

                // Serwisy
                services.AddSingleton<JsonDataStore>();
                services.AddSingleton<EconomyService>();
                services.AddSingleton<RoleShopService>();
                services.AddSingleton<VoiceTrackingService>();
                services.AddSingleton<MiningService>();
                services.AddSingleton<ShellGameService>();
                services.AddSingleton<ReactionRoleService>();

                // Komendy
                services.AddSingleton<Commands.EconomyCommands>();
                services.AddSingleton<Commands.AdminCommands>();

                // Bot
                services.AddSingleton<BotClient>();
                services.AddHostedService<BotHostedService>();
            })
            .Build();

        await host.RunAsync();
    }
}
