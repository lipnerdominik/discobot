using Discord.Interactions;
using Discord;
using DiscordEconomyBot.Services;
using DiscordEconomyBot.Models;
using Discord.WebSocket;

namespace DiscordEconomyBot.Commands;

public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly EconomyService _economyService;
    private readonly RoleShopService _roleShopService;
    private readonly MiningService _miningService;
    private readonly ShellGameService _shellGameService;

    public SlashCommands(EconomyService economyService, RoleShopService roleShopService, MiningService miningService, ShellGameService shellGameService)
    {
        _economyService = economyService;
        _roleShopService = roleShopService;
        _miningService = miningService;
        _shellGameService = shellGameService;
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
        await RespondAsync(embed: embed);
    }

    [SlashCommand("daily", "Odbierz codzienn¹ nagrodê")]
    public async Task Daily()
    {
        var result = _economyService.ClaimDaily(Context.User.Id);
        await RespondAsync(result.success ? $":white_check_mark: {Context.User.Mention} odebra³ codzienn¹ nagrodê!" : $":x: {result.message}");
    }

    [SlashCommand("top", "Zobacz ranking najbogatszych")]
    public async Task Top()
    {
        var top = _economyService.GetTopUsers(10);
        var desc = string.Join("\n", top.Select((u, i) => $"{i + 1}. <@{u.UserId}> - **{u.Balance}** monet"));
        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle(":trophy: Top 10")
            .WithDescription(desc)
            .Build();
        await RespondAsync(embed: embed);
    }

    [SlashCommand("sklep", "Zobacz dostêpne rangi do kupienia")]
    public async Task Sklep()
    {
        var roles = _roleShopService.GetAllRoles();
        
        if (!roles.Any())
        {
            await RespondAsync(":shopping_cart: Sklep jest obecnie pusty!");
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle(":shopping_cart: Sklep z Rangami")
            .WithDescription("U¿yj `/kup @Ranga` aby kupiæ rangê");

        foreach (var role in roles)
        {
            var description = string.IsNullOrEmpty(role.Description) 
                ? "Brak opisu" 
                : role.Description;
            embed.AddField($"<@&{role.RoleId}> - {role.RoleName}", 
                $":moneybag: Cena: **{role.Price}** monet\n{description}", 
                inline: false);
        }

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("kup", "Kup rangê ze sklepu")]
    public async Task Kup([Summary("ranga", "Ranga do kupienia")] SocketRole role)
    {
        if (Context.User is not SocketGuildUser guildUser)
        {
            await RespondAsync(":x: Ta komenda dzia³a tylko na serwerze!", ephemeral: true);
            return;
        }

        var result = await _roleShopService.BuyRole(guildUser, role.Id);
        await RespondAsync(result.success ? $":white_check_mark: {guildUser.Mention} {result.message}" : $":x: {result.message}");
    }

    [SlashCommand("statystyki", "Zobacz swoje szczegó³owe statystyki")]
    public async Task Statystyki()
    {
        var stats = _economyService.GetUserStats(Context.User.Id);

        var voiceHours = stats.VoiceTime.TotalHours;
        var voiceMinutes = stats.VoiceTime.Minutes;

        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithTitle($":bar_chart: Statystyki {Context.User.Username}")
            .AddField(":moneybag: Saldo", $"**{stats.Balance}** monet", inline: true)
            .AddField(":pencil: Wiadomoœci", $"**{stats.MessageCount}**", inline: true)
            .AddField(":microphone2: Czas na voice", $"**{(int)voiceHours}h {voiceMinutes}m**", inline: true)
            .AddField(":busts_in_silhouette: Zaproszenia", $"**{stats.InviteCount}**", inline: true)
            .AddField(":bar_chart: Ankiety", $"**{stats.PollParticipation}**", inline: true)
            .AddField(":tada: Wydarzenia", $"**{stats.EventParticipation}**", inline: true)
            .AddField(":calendar: Dni aktywnoœci", $"**{stats.DaysActive}**", inline: true)
            .AddField(":trophy: Osi¹gniêcia", $"**{stats.Achievements.Count}**", inline: true)
            .WithFooter($"Ostatnia aktywnoœæ: {stats.LastActivity:dd.MM.yyyy HH:mm}")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("osiagniecia", "Zobacz swoje osi¹gniêcia")]
    public async Task Osiagniecia()
    {
        var stats = _economyService.GetUserStats(Context.User.Id);

        var achievementDescriptions = new Dictionary<string, string>
        {
            { "100_messages", ":pencil: Wys³ano 100 wiadomoœci" },
            { "500_messages", ":pencil::sparkles: Wys³ano 500 wiadomoœci" },
            { "1000_messages", ":pencil::star2: Wys³ano 1000 wiadomoœci" },
            { "7_days_active", ":calendar: 7 dni aktywnoœci" },
            { "30_days_active", ":calendar::star2: 30 dni aktywnoœci" }
        };

        var description = stats.Achievements.Count == 0 
            ? "Nie masz jeszcze ¿adnych osi¹gniêæ!" 
            : "";

        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle($":trophy: Osi¹gniêcia {Context.User.Username}")
            .WithDescription(description);

        if (stats.Achievements.Any())
        {
            foreach (var achievement in stats.Achievements)
            {
                if (achievementDescriptions.TryGetValue(achievement, out var desc))
                {
                    embed.AddField(desc, ":white_check_mark: Ukoñczone", inline: false);
                }
            }
        }

        var totalAchievements = achievementDescriptions.Count;
        embed.WithFooter($"Zdobyto {stats.Achievements.Count}/{totalAchievements} osi¹gniêæ");

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("kop", "Wykop cenne surowce!")]
    public async Task Kop()
    {
        var (success, message, item, bonusItem) = _miningService.Mine(Context.User.Id);
        
        if (!success)
        {
            // SprawdŸ czy to atak goblina (wiadomoœæ zawiera "Goblin")
            if (message.Contains("Goblin"))
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Red)
                    .WithTitle(":imp: Atak Goblina!")
                    .WithDescription(message)
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithFooter("Nastêpnym razem bêdziesz mia³ wiêcej szczêœcia!")
                    .WithCurrentTimestamp()
                    .Build();

                await RespondAsync(embed: embed);
                return;
            }

            // Cooldown lub inny b³¹d
            await RespondAsync($":x: {message}", ephemeral: true);
            return;
        }

        // Okreœl kolor - jeœli jest bonus, u¿yj koloru bonusowego przedmiotu, w przeciwnym razie g³ównego
        var embedColor = bonusItem != null ? Color.Gold : GetRarityColor(item!.Rarity);

        var resultEmbed = new EmbedBuilder()
            .WithColor(embedColor)
            .WithTitle(bonusItem != null ? ":pickaxe::sparkles: Kopanie surowców - PODWÓJNY DROP!" : ":pickaxe: Kopanie surowców")
            .WithDescription(message)
            .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithFooter("U¿yj /ekwipunek aby zobaczyæ swoje przedmioty")
            .WithCurrentTimestamp()
            .Build();

        await RespondAsync(embed: resultEmbed);
    }

    [SlashCommand("ekwipunek", "Zobacz swój ekwipunek")]
    public async Task Ekwipunek()
    {
        var inventory = _miningService.GetInventory(Context.User.Id);
        var (totalValue, items) = _miningService.GetInventoryValue(Context.User.Id);

        var embed = new EmbedBuilder()
            .WithColor(Color.DarkGreen)
            .WithTitle($":school_satchel: Ekwipunek {Context.User.Username}")
            .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());

        // Informacja o górniku
        var currentUpgrade = _miningService.GetCurrentUpgrade(Context.User.Id);
        var nextUpgrade = _miningService.GetNextUpgrade(Context.User.Id);
        
        if (currentUpgrade != null)
        {
            embed.AddField(":man_mage: Twój górnik", 
                $"Poziom: **{currentUpgrade.Level}**\n{currentUpgrade.Description}", 
                inline: false);
        }
        else
        {
            embed.AddField(":man_mage: Górnik", 
                "Nie masz jeszcze górnika! U¿yj `/ulepszgornika` aby go kupiæ.", 
                inline: false);
        }

        if (!items.Any())
        {
            embed.WithDescription("Twój ekwipunek jest pusty! U¿yj `/kop` aby wykopaæ surowce.");
        }
        else
        {
            embed.WithDescription($"£¹czna wartoœæ przedmiotów: **{totalValue}** monet");
            
            var allItems = _miningService.GetAllItems();
            var groupedByRarity = items
                .Select(kvp => (
                    name: kvp.Key,
                    count: kvp.Value.count,
                    value: kvp.Value.value,
                    item: allItems.First(i => i.Name == kvp.Key)
                ))
                .GroupBy(x => x.item.Rarity)
                .OrderBy(g => g.Key);

            foreach (var group in groupedByRarity)
            {
                var rarityText = GetRarityText(group.Key);
                var itemsList = string.Join("\n", group.Select(x => 
                    $"{x.item.Emoji} **{x.name}** x{x.count} - {x.value} monet"));
                
                embed.AddField(rarityText, itemsList, inline: false);
            }
        }

        embed.AddField(":pick: Statystyki kopania", 
            $"Wykopane przedmioty: **{inventory.TotalMiningCount}**", 
            inline: true);

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("ulepszgornika", "Ulepsz swojego górnika")]
    public async Task UlepszGornika()
    {
        var nextUpgrade = _miningService.GetNextUpgrade(Context.User.Id);
        
        if (nextUpgrade == null)
        {
            await RespondAsync(":white_check_mark: Osi¹gn¹³eœ maksymalny poziom górnika!", ephemeral: true);
            return;
        }

        var (success, message) = _miningService.UpgradeMiner(Context.User.Id);

        if (!success)
        {
            await RespondAsync($":x: {message}", ephemeral: true);
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Gold)
            .WithTitle(":man_mage::sparkles: Ulepszenie górnika!")
            .WithDescription(message)
            .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithFooter("Twoja szansa na podwójny drop wzros³a!")
            .WithCurrentTimestamp()
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("gornik", "Zobacz informacje o ulepszeniach górnika")]
    public async Task Gornik()
    {
        var inventory = _miningService.GetInventory(Context.User.Id);
        var currentUpgrade = _miningService.GetCurrentUpgrade(Context.User.Id);
        var nextUpgrade = _miningService.GetNextUpgrade(Context.User.Id);
        var allUpgrades = _miningService.GetAllUpgrades();

        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithTitle(":man_mage: System Ulepszeñ Górnika")
            .WithDescription("Ulepszenia górnika zwiêkszaj¹ szansê na wykopanie **2 przedmiotów naraz**!");

        // Obecny poziom
        if (currentUpgrade != null)
        {
            embed.AddField(":white_check_mark: Twój aktualny poziom", 
                $"**Poziom {currentUpgrade.Level}**\n{currentUpgrade.Description}", 
                inline: false);
        }
        else
        {
            embed.AddField(":x: Brak górnika", 
                "Nie masz jeszcze górnika! Kup pierwszy poziom poni¿ej.", 
                inline: false);
        }

        // Nastêpny poziom
        if (nextUpgrade != null)
        {
            var balance = _economyService.GetBalance(Context.User.Id);
            var canAfford = balance >= nextUpgrade.Cost ? ":white_check_mark:" : ":x:";
            
            embed.AddField($":arrow_up: Nastêpny poziom ({canAfford})", 
                $"**Poziom {nextUpgrade.Level}** - {nextUpgrade.Cost} monet\n{nextUpgrade.Description}\nU¿yj `/ulepszgornika` aby kupiæ", 
                inline: false);
        }

        // Wszystkie poziomy
        var upgradesList = string.Join("\n", allUpgrades.Select(u => 
        {
            var status = u.Level == inventory.MinerLevel ? ":white_check_mark: " : 
                        u.Level < inventory.MinerLevel ? ":white_check_mark: " : "";
            return $"{status}**Poziom {u.Level}** ({u.Cost} monet) - {u.DoubleDropChance}% szans";
        }));

        embed.AddField(":gem: Wszystkie poziomy", upgradesList, inline: false);
        embed.WithFooter("Im wy¿szy poziom, tym czêœciej wykopiesz 2 przedmioty!");

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("pomoc", "Wyœwietl listê wszystkich komend")]
    public async Task Pomoc()
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle(":book: Lista Komend")
            .WithDescription("Wszystkie dostêpne komendy bota:")
            .AddField(":moneybag: Komendy Ekonomiczne", 
                "`/saldo` - SprawdŸ swoje saldo\n" +
                "`/daily` - Odbierz codzienn¹ nagrodê\n" +
                "`/top` - Zobacz ranking najbogatszych\n" +
                "`/sklep` - Zobacz dostêpne rangi\n" +
                "`/kup @Ranga` - Kup rangê ze sklepu", 
                inline: false)
            .AddField(":pickaxe: Komendy Kopania", 
                "`/kop` - Wykop surowce (cooldown: 60s)\n" +
                "`/ekwipunek` - Zobacz swój ekwipunek\n" +
                "`/surowce` - Lista wszystkich surowców\n" +
                "`/sprzedaj <przedmiot> <iloœæ>` - Sprzedaj przedmiot\n" +
                "`/sprzedajwszystko` - Sprzedaj wszystkie przedmioty", 
                inline: false)
            .AddField(":man_mage: System Górnika", 
                "`/gornik` - Zobacz info o ulepszeniach\n" +
                "`/ulepszgornika` - Ulepsz górnika (podwójny drop!)", 
                inline: false)
            .AddField(":game_die: Gry hazardowe", 
                "`/kubki <zak³ad>` - Gra w kubki (kliknij przycisk aby wybraæ!)", 
                inline: false)
            .AddField(":bar_chart: Statystyki", 
                "`/statystyki` - Zobacz swoje statystyki\n" +
                "`/osiagniecia` - Zobacz swoje osi¹gniêcia", 
                inline: false)
            .AddField(":gem: Sposoby zarabiania monet", 
                ":pencil: Pisanie wiadomoœci - 5 monet\n" +
                ":pickaxe: Kopanie surowców - 5-500 monet\n" +
                ":sparkles: Podwójny drop z górnikiem - do 1000 monet!\n" +
                ":game_die: Gra w kubki - wygraj x2 zak³adu!\n" +
                ":microphone2: Przebywanie na voice - 2 monety/min\n" +
                ":busts_in_silhouette: Zapraszanie u¿ytkowników - 100 monet\n" +
                ":bar_chart: Udzia³ w ankietach - 10 monet\n" +
                ":tada: Uczestnictwo w wydarzeniach - 50 monet\n" +
                ":gift: Codzienna nagroda - 100 monet", 
                inline: false)
            .WithFooter("Bot Discord Economy | Discord.Net")
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("kubki", "Zagraj w grê Shell Game - zgadnij pod którym kubkiem jest kulka!")]
    public async Task Kubki([Summary("zak³ad", "Iloœæ monet do postawienia (10-1000)")] long betAmount)
    {
        var (success, message, correctCup) = _shellGameService.StartGame(Context.User.Id, betAmount);

        if (!success)
        {
            await RespondAsync($":x: {message}", ephemeral: true);
            return;
        }

        var embed = new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithTitle(":cup_with_straw: Gra w Kubki!")
            .WithDescription(
                "Obserwuj uwa¿nie! Pod jednym z kubków jest kulka :red_circle:\n\n" +
                ":one: :cup_with_straw:     :two: :cup_with_straw:     :three: :cup_with_straw:\n\n" +
                $"{message}\n\n" +
                "Wybierz kubek klikaj¹c odpowiedni przycisk poni¿ej!")
            .AddField(":moneybag: Twój zak³ad", $"**{betAmount}** monet", inline: true)
            .AddField(":trophy: Mo¿liwa wygrana", $"**{betAmount * 2}** monet", inline: true)
            .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
            .WithFooter("Masz 5 minut na wybór kubka!")
            .WithCurrentTimestamp()
            .Build();

        // Stwórz buttony do wyboru kubka
        var components = new ComponentBuilder()
            .WithButton("Kubek 1", $"shellgame_1_{Context.User.Id}", ButtonStyle.Primary)
            .WithButton("Kubek 2", $"shellgame_2_{Context.User.Id}", ButtonStyle.Primary)
            .WithButton("Kubek 3", $"shellgame_3_{Context.User.Id}", ButtonStyle.Primary)
            .Build();

        await RespondAsync(embed: embed, components: components);
    }

    [SlashCommand("surowce", "Zobacz listê wszystkich dostêpnych surowców")]
    public async Task Surowce()
    {
        var items = _miningService.GetAllItems();
        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithTitle(":gem: Lista surowców")
            .WithDescription("Wszystkie surowce które mo¿esz wykopaæ:");

        var groupedByRarity = items
            .GroupBy(i => i.Rarity)
            .OrderBy(g => g.Key);

        foreach (var group in groupedByRarity)
        {
            var rarityText = GetRarityText(group.Key);
            var itemsList = string.Join("\n", group.Select(i => 
                $"{i.Emoji} **{i.Name}** - {i.Value} monet ({i.DropChance:F1}%)"));
            
            embed.AddField(rarityText, itemsList, inline: false);
        }

        embed.WithFooter("U¿yj /kop aby wykopaæ surowce (cooldown: 60s)");

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("sprzedaj", "Sprzedaj przedmioty z ekwipunku")]
    public async Task Sprzedaj(
        [Summary("przedmiot", "Nazwa przedmiotu do sprzedania")] string itemName,
        [Summary("iloœæ", "Iloœæ do sprzedania")] int quantity = 1)
    {
        var inventory = _miningService.GetInventory(Context.User.Id);
        
        if (!inventory.Items.ContainsKey(itemName))
        {
            await RespondAsync($":x: Nie masz przedmiotu **{itemName}** w ekwipunku!", ephemeral: true);
            return;
        }

        var availableCount = inventory.Items[itemName];

        if (quantity > availableCount)
        {
            await RespondAsync($":x: Masz tylko **{availableCount}x {itemName}**!", ephemeral: true);
            return;
        }

        var allItems = _miningService.GetAllItems();
        var item = allItems.FirstOrDefault(i => i.Name == itemName);
        
        if (item == null)
        {
            await RespondAsync($":x: Nieznany przedmiot!", ephemeral: true);
            return;
        }

        var totalValue = item.Value * quantity;
        _miningService.SellItem(Context.User.Id, itemName, quantity);

        await RespondAsync($":white_check_mark: Sprzedano **{quantity}x {item.Emoji} {itemName}** za **{totalValue}** monet!");
    }

    [SlashCommand("sprzedajwszystko", "Sprzedaj wszystkie przedmioty z ekwipunku")]
    public async Task SprzedajWszystko()
    {
        var inventory = _miningService.GetInventory(Context.User.Id);
        
        if (!inventory.Items.Any())
        {
            await RespondAsync(":x: Twój ekwipunek jest pusty!", ephemeral: true);
            return;
        }

        var (totalValue, _) = _miningService.GetInventoryValue(Context.User.Id);
        _miningService.SellAll(Context.User.Id);

        await RespondAsync($":white_check_mark: Sprzedano wszystkie przedmioty za **{totalValue}** monet!");
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.LightGrey,
            Rarity.Uncommon => Color.Green,
            Rarity.Rare => Color.Blue,
            Rarity.Epic => Color.Purple,
            Rarity.Legendary => Color.Orange,
            _ => Color.Default
        };
    }

    private string GetRarityText(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => ":white_circle: Pospolite",
            Rarity.Uncommon => ":green_circle: Rzadkie",
            Rarity.Rare => ":blue_circle: Bardzo rzadkie",
            Rarity.Epic => ":purple_circle: Epickie",
            Rarity.Legendary => ":orange_circle: Legendarne",
            _ => "Nieznane"
        };
    }

    [Group("admin", "Komendy administratorskie")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly EconomyService _economyService;
        private readonly RoleShopService _roleShopService;
        private readonly ReactionRoleService _reactionRoleService;

        public AdminCommands(EconomyService economyService, RoleShopService roleShopService, ReactionRoleService reactionRoleService)
        {
            _economyService = economyService;
            _roleShopService = roleShopService;
            _reactionRoleService = reactionRoleService;
        }

        [SlashCommand("dodajmonety", "Dodaj monety u¿ytkownikowi")]
        public async Task DodajMonety(
            [Summary("u¿ytkownik", "U¿ytkownik, któremu chcesz dodaæ monety")] SocketGuildUser user,
            [Summary("iloœæ", "Iloœæ monet do dodania")] long amount)
        {
            if (amount <= 0)
            {
                await RespondAsync(":x: Iloœæ monet musi byæ wiêksza od 0!", ephemeral: true);
                return;
            }

            _economyService.AddCoins(user.Id, amount, $"Admin {Context.User.Username} doda³ monety");
            await RespondAsync($":white_check_mark: {Context.User.Mention} doda³ **{amount}** monet dla {user.Mention}!");
        }

        [SlashCommand("usunmonety", "Usuñ monety u¿ytkownikowi")]
        public async Task UsunMonety(
            [Summary("u¿ytkownik", "U¿ytkownik, któremu chcesz usun¹æ monety")] SocketGuildUser user,
            [Summary("iloœæ", "Iloœæ monet do usuniêcia")] long amount)
        {
            if (amount <= 0)
            {
                await RespondAsync(":x: Iloœæ monet musi byæ wiêksza od 0!", ephemeral: true);
                return;
            }

            var success = _economyService.RemoveCoins(user.Id, amount, $"Admin {Context.User.Username} usun¹³ monety");
            
            if (success)
                await RespondAsync($":white_check_mark: {Context.User.Mention} usun¹³ **{amount}** monet od {user.Mention}!");
            else
                await RespondAsync($":x: {user.Mention} nie ma wystarczaj¹co monet!", ephemeral: true);
        }

        [SlashCommand("dodajrange", "Dodaj rangê do sklepu")]
        public async Task DodajRange(
            [Summary("ranga", "Ranga do dodania do sklepu")] SocketRole role,
            [Summary("cena", "Cena rangi w monetach")] long price,
            [Summary("opis", "Opis rangi (opcjonalnie)")] string description = "")
        {
            if (price <= 0)
            {
                await RespondAsync(":x: Cena musi byæ wiêksza od 0!", ephemeral: true);
                return;
            }

            _roleShopService.AddRole(role.Id, role.Name, price, description);
            await RespondAsync($":white_check_mark: {Context.User.Mention} doda³ rangê {role.Mention} do sklepu za **{price}** monet!");
        }

        [SlashCommand("usunrange", "Usuñ rangê ze sklepu")]
        public async Task UsunRange(
            [Summary("ranga", "Ranga do usuniêcia ze sklepu")] SocketRole role)
        {
            _roleShopService.RemoveRole(role.Id);
            await RespondAsync($":white_check_mark: {Context.User.Mention} usun¹³ rangê {role.Mention} ze sklepu!");
        }

        [SlashCommand("wydarzenie", "Przyznaj nagrody za wydarzenie")]
        public async Task Wydarzenie(
            [Summary("u¿ytkownik1", "Pierwszy uczestnik wydarzenia")] SocketGuildUser user1,
            [Summary("u¿ytkownik2", "Drugi uczestnik wydarzenia (opcjonalnie)")] SocketGuildUser? user2 = null,
            [Summary("u¿ytkownik3", "Trzeci uczestnik wydarzenia (opcjonalnie)")] SocketGuildUser? user3 = null,
            [Summary("u¿ytkownik4", "Czwarty uczestnik wydarzenia (opcjonalnie)")] SocketGuildUser? user4 = null,
            [Summary("u¿ytkownik5", "Pi¹ty uczestnik wydarzenia (opcjonalnie)")] SocketGuildUser? user5 = null)
        {
            var users = new List<SocketGuildUser> { user1 };
            if (user2 != null) users.Add(user2);
            if (user3 != null) users.Add(user3);
            if (user4 != null) users.Add(user4);
            if (user5 != null) users.Add(user5);

            foreach (var user in users)
            {
                _economyService.HandleEventParticipation(user.Id);
            }

            var userMentions = string.Join(", ", users.Select(u => u.Mention));
            await RespondAsync($":white_check_mark: {Context.User.Mention} przyzna³ nagrody za wydarzenie dla {users.Count} u¿ytkowników! :tada:\n{userMentions}");
        }

        [SlashCommand("pomoc", "Wyœwietl listê komend administratora")]
        public async Task AdminPomoc()
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle(":wrench: Pomoc - Komendy Administratora")
                .WithDescription("Lista dostêpnych komend administracyjnych:")
                .AddField(":moneybag: /admin dodajmonety", "Dodaj monety u¿ytkownikowi", inline: false)
                .AddField(":money_with_wings: /admin usunmonety", "Usuñ monety u¿ytkownikowi", inline: false)
                .AddField(":performing_arts: /admin dodajrange", "Dodaj rangê do sklepu", inline: false)
                .AddField(":wastebasket: /admin usunrange", "Usuñ rangê ze sklepu", inline: false)
                .AddField(":tada: /admin wydarzenie", "Przyznaj nagrody za wydarzenie", inline: false)
                .AddField(":star: /admin reactionrole", "Dodaj Reaction Role do wiadomoœci", inline: false)
                .AddField(":x: /admin usunreactionrole", "Usuñ Reaction Role", inline: false)
                .AddField(":information_source: /admin listareactionroles", "Zobacz wszystkie Reaction Roles", inline: false)
                .WithFooter("Tylko administratorzy mog¹ u¿ywaæ tych komend")
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("reactionrole", "Dodaj Reaction Role - klikniêcie reakcji daje rangê")]
        public async Task ReactionRole(
            [Summary("wiadomoœæ", "ID wiadomoœci")] string messageId,
            [Summary("emoji", "Emoji reakcji (np. ?? lub :custom_emoji:)")] string emoji,
            [Summary("ranga", "Ranga do przypisania")] SocketRole role)
        {
            if (!ulong.TryParse(messageId, out var msgId))
            {
                await RespondAsync(":x: Nieprawid³owe ID wiadomoœci!", ephemeral: true);
                return;
            }

            if (Context.Channel is not IMessageChannel channel)
            {
                await RespondAsync(":x: Nie mogê uzyskaæ dostêpu do kana³u!", ephemeral: true);
                return;
            }

            var (success, message) = await _reactionRoleService.AddReactionRole(
                msgId, 
                Context.Channel.Id, 
                emoji, 
                role.Id, 
                role.Name);

            if (success)
            {
                await RespondAsync($":white_check_mark: {message}", ephemeral: true);
            }
            else
            {
                await RespondAsync($":x: {message}", ephemeral: true);
            }
        }

        [SlashCommand("usunreactionrole", "Usuñ Reaction Role")]
        public async Task UsunReactionRole(
            [Summary("wiadomoœæ", "ID wiadomoœci")] string messageId,
            [Summary("emoji", "Emoji reakcji")] string emoji)
        {
            if (!ulong.TryParse(messageId, out var msgId))
            {
                await RespondAsync(":x: Nieprawid³owe ID wiadomoœci!", ephemeral: true);
                return;
            }

            var (success, message) = await _reactionRoleService.RemoveReactionRole(msgId, emoji);

            if (success)
            {
                await RespondAsync($":white_check_mark: {message}", ephemeral: true);
            }
            else
            {
                await RespondAsync($":x: {message}", ephemeral: true);
            }
        }

        [SlashCommand("listareactionroles", "Zobacz wszystkie Reaction Roles")]
        public async Task ListaReactionRoles()
        {
            var reactionRoles = _reactionRoleService.GetAllReactionRoles();

            if (!reactionRoles.Any())
            {
                await RespondAsync(":information_source: Brak Reaction Roles!", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle(":star: Lista Reaction Roles")
                .WithDescription($"£¹cznie: {reactionRoles.Count} Reaction Roles");

            foreach (var rr in reactionRoles.Take(25)) // Discord limit to 25 fields
            {
                embed.AddField(
                    $"{rr.Emoji} ? {rr.RoleName}",
                    $"Wiadomoœæ: `{rr.MessageId}`\nKana³: <#{rr.ChannelId}>",
                    inline: false);
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
