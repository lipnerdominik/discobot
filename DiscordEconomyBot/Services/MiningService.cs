using DiscordEconomyBot.Data;
using DiscordEconomyBot.Models;

namespace DiscordEconomyBot.Services;

public class MiningService
{
    private readonly JsonDataStore _dataStore;
    private readonly EconomyService _economyService;
    private readonly List<MiningItem> _miningItems;
    private readonly List<MinerUpgrade> _minerUpgrades;
    private readonly Random _random = new();
    private readonly int _miningCooldownSeconds = 60;

    public MiningService(JsonDataStore dataStore, EconomyService economyService)
    {
        _dataStore = dataStore;
        _economyService = economyService;
        _miningItems = InitializeMiningItems();
        _minerUpgrades = InitializeMinerUpgrades();
    }

    private List<MinerUpgrade> InitializeMinerUpgrades()
    {
        return new List<MinerUpgrade>
        {
            new MinerUpgrade { Level = 1, Cost = 500, DoubleDropChance = 10.0, Description = "Podstawowy górnik - 10% szans na podwójny drop" },
            new MinerUpgrade { Level = 2, Cost = 1500, DoubleDropChance = 20.0, Description = "Ulepszony górnik - 20% szans na podwójny drop" },
            new MinerUpgrade { Level = 3, Cost = 3500, DoubleDropChance = 35.0, Description = "Zaawansowany górnik - 35% szans na podwójny drop" },
            new MinerUpgrade { Level = 4, Cost = 7500, DoubleDropChance = 50.0, Description = "Mistrzowski górnik - 50% szans na podwójny drop" },
            new MinerUpgrade { Level = 5, Cost = 15000, DoubleDropChance = 70.0, Description = "Legendarny górnik - 70% szans na podwójny drop" }
        };
    }

    private List<MiningItem> InitializeMiningItems()
    {
        return new List<MiningItem>
        {
            // Pospolite (70% szans ³¹cznie)
            new MiningItem { Name = "Kamieñ", Emoji = ":rock:", Value = 5, DropChance = 30.0, Rarity = Rarity.Common },
            new MiningItem { Name = "Wêgiel", Emoji = ":black_circle:", Value = 10, DropChance = 25.0, Rarity = Rarity.Common },
            new MiningItem { Name = "¯elazo", Emoji = ":chains:", Value = 15, DropChance = 15.0, Rarity = Rarity.Common },
            
            // Rzadkie (20% szans ³¹cznie)
            new MiningItem { Name = "Z³oto", Emoji = ":yellow_circle:", Value = 30, DropChance = 10.0, Rarity = Rarity.Uncommon },
            new MiningItem { Name = "Redstone", Emoji = ":red_circle:", Value = 35, DropChance = 7.0, Rarity = Rarity.Uncommon },
            new MiningItem { Name = "Lapis Lazuli", Emoji = ":blue_circle:", Value = 40, DropChance = 3.0, Rarity = Rarity.Uncommon },
            
            // Bardzo rzadkie (7% szans ³¹cznie)
            new MiningItem { Name = "Szmaragd", Emoji = ":green_circle:", Value = 75, DropChance = 3.5, Rarity = Rarity.Rare },
            new MiningItem { Name = "Ametyst", Emoji = ":purple_circle:", Value = 80, DropChance = 2.0, Rarity = Rarity.Rare },
            new MiningItem { Name = "Obsydian", Emoji = ":black_large_square:", Value = 90, DropChance = 1.5, Rarity = Rarity.Rare },
            
            // Epickie (2.5% szans ³¹cznie)
            new MiningItem { Name = "Diament", Emoji = ":gem:", Value = 150, DropChance = 1.5, Rarity = Rarity.Epic },
            new MiningItem { Name = "Rubin", Emoji = ":red_square:", Value = 175, DropChance = 1.0, Rarity = Rarity.Epic },
            
            // Legendarne (0.5% szans)
            new MiningItem { Name = "Netherite", Emoji = ":brown_square:", Value = 300, DropChance = 0.3, Rarity = Rarity.Legendary },
            new MiningItem { Name = "Gwiezdny Kryszta³", Emoji = ":star2:", Value = 500, DropChance = 0.2, Rarity = Rarity.Legendary }
        };
    }

    public (bool success, string message, MiningItem? item, MiningItem? bonusItem) Mine(ulong userId)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        var now = DateTime.UtcNow;

        // SprawdŸ cooldown
        if ((now - inventory.LastMining).TotalSeconds < _miningCooldownSeconds)
        {
            var timeLeft = _miningCooldownSeconds - (int)(now - inventory.LastMining).TotalSeconds;
            return (false, $"Musisz poczekaæ jeszcze {timeLeft}s przed nastêpnym kopaniem!", null, null);
        }

        // 15% szans na atak goblina
        var goblinAttackChance = _random.NextDouble() * 100.0;
        if (goblinAttackChance <= 15.0)
        {
            var stolenAmount = _random.Next(70, 151); // 70-150 monet
            var currentBalance = _economyService.GetBalance(userId);
            
            inventory.LastMining = now;
            _dataStore.UpdateUserInventory(inventory);

            if (currentBalance >= stolenAmount)
            {
                _economyService.RemoveCoins(userId, stolenAmount, "Okradziony przez goblina");
                return (false, $":imp: **Goblin ciê zaatakowa³!** Ukrad³ ci **{stolenAmount}** monet!", null, null);
            }
            else if (currentBalance > 0)
            {
                _economyService.RemoveCoins(userId, currentBalance, "Okradziony przez goblina");
                return (false, $":imp: **Goblin ciê zaatakowa³!** Ukrad³ ci wszystkie monety (**{currentBalance}** monet)!", null, null);
            }
            else
            {
                return (false, ":imp: **Goblin ciê zaatakowa³!** Na szczêœcie nie mia³eœ przy sobie monet!", null, null);
            }
        }

        // Losuj pierwszy przedmiot
        var item = GetRandomItem();
        
        // Dodaj przedmiot do ekwipunku
        if (!inventory.Items.ContainsKey(item.Name))
            inventory.Items[item.Name] = 0;
        
        inventory.Items[item.Name]++;
        inventory.TotalMiningCount++;

        // Dodaj monety za przedmiot
        var totalValue = item.Value;
        _economyService.AddCoins(userId, item.Value, $"Wykopanie: {item.Name}");

        MiningItem? bonusItem = null;

        // SprawdŸ czy gracz ma ulepszenie górnika i czy trafi bonusowy przedmiot
        if (inventory.MinerLevel > 0)
        {
            var minerUpgrade = _minerUpgrades.FirstOrDefault(u => u.Level == inventory.MinerLevel);
            if (minerUpgrade != null)
            {
                var doubleDropRoll = _random.NextDouble() * 100.0;
                if (doubleDropRoll <= minerUpgrade.DoubleDropChance)
                {
                    // Losuj drugi przedmiot
                    bonusItem = GetRandomItem();
                    
                    if (!inventory.Items.ContainsKey(bonusItem.Name))
                        inventory.Items[bonusItem.Name] = 0;
                    
                    inventory.Items[bonusItem.Name]++;
                    totalValue += bonusItem.Value;
                    _economyService.AddCoins(userId, bonusItem.Value, $"Wykopanie (bonus): {bonusItem.Name}");
                }
            }
        }

        inventory.LastMining = now;
        _dataStore.UpdateUserInventory(inventory);

        var rarityText = GetRarityText(item.Rarity);
        var message = bonusItem != null
            ? $"Wykopa³eœ {item.Emoji} **{item.Name}** ({rarityText})!\n:sparkles: **BONUS!** {bonusItem.Emoji} **{bonusItem.Name}** ({GetRarityText(bonusItem.Rarity)})\n£¹czna wartoœæ: **{totalValue}** monet"
            : $"Wykopa³eœ {item.Emoji} **{item.Name}** ({rarityText})!\nWartoœæ: **{item.Value}** monet";

        return (true, message, item, bonusItem);
    }

    private MiningItem GetRandomItem()
    {
        var roll = _random.NextDouble() * 100.0;
        var cumulative = 0.0;

        foreach (var item in _miningItems)
        {
            cumulative += item.DropChance;
            if (roll <= cumulative)
                return item;
        }

        return _miningItems.First();
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

    public UserInventory GetInventory(ulong userId)
    {
        return _dataStore.GetUserInventory(userId);
    }

    public List<MiningItem> GetAllItems()
    {
        return _miningItems;
    }

    public (long totalValue, Dictionary<string, (int count, long value)> items) GetInventoryValue(ulong userId)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        var result = new Dictionary<string, (int count, long value)>();
        long totalValue = 0;

        foreach (var (itemName, count) in inventory.Items)
        {
            var item = _miningItems.FirstOrDefault(i => i.Name == itemName);
            if (item != null)
            {
                var value = item.Value * count;
                result[itemName] = (count, value);
                totalValue += value;
            }
        }

        return (totalValue, result);
    }

    public bool SellItem(ulong userId, string itemName, int quantity)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        
        if (!inventory.Items.ContainsKey(itemName) || inventory.Items[itemName] < quantity)
            return false;

        var item = _miningItems.FirstOrDefault(i => i.Name == itemName);
        if (item == null)
            return false;

        inventory.Items[itemName] -= quantity;
        if (inventory.Items[itemName] == 0)
            inventory.Items.Remove(itemName);

        _dataStore.UpdateUserInventory(inventory);
        _economyService.AddCoins(userId, item.Value * quantity, $"Sprzeda¿: {quantity}x {itemName}");

        return true;
    }

    public bool SellAll(ulong userId)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        
        if (!inventory.Items.Any())
            return false;

        long totalValue = 0;
        var itemsToSell = new List<string>(inventory.Items.Keys);

        foreach (var itemName in itemsToSell)
        {
            var item = _miningItems.FirstOrDefault(i => i.Name == itemName);
            if (item != null)
            {
                totalValue += item.Value * inventory.Items[itemName];
            }
        }

        inventory.Items.Clear();
        _dataStore.UpdateUserInventory(inventory);
        _economyService.AddCoins(userId, totalValue, "Sprzeda¿ wszystkich przedmiotów");

        return true;
    }

    public (bool success, string message) UpgradeMiner(ulong userId)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        var currentLevel = inventory.MinerLevel;
        var nextLevel = currentLevel + 1;

        if (nextLevel > _minerUpgrades.Count)
        {
            return (false, "Osi¹gn¹³eœ maksymalny poziom górnika!");
        }

        var upgrade = _minerUpgrades.FirstOrDefault(u => u.Level == nextLevel);
        if (upgrade == null)
        {
            return (false, "B³¹d: Nie znaleziono ulepszenia!");
        }

        var balance = _economyService.GetBalance(userId);
        if (balance < upgrade.Cost)
        {
            return (false, $"Nie masz wystarczaj¹co monet! Potrzebujesz **{upgrade.Cost}** monet (masz {balance}).");
        }

        _economyService.RemoveCoins(userId, upgrade.Cost, $"Zakup ulepszenia górnika poziom {nextLevel}");
        inventory.MinerLevel = nextLevel;
        _dataStore.UpdateUserInventory(inventory);

        return (true, $"Ulepszono górnika do poziomu **{nextLevel}**!\n{upgrade.Description}");
    }

    public List<MinerUpgrade> GetAllUpgrades()
    {
        return _minerUpgrades;
    }

    public MinerUpgrade? GetCurrentUpgrade(ulong userId)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        if (inventory.MinerLevel == 0)
            return null;

        return _minerUpgrades.FirstOrDefault(u => u.Level == inventory.MinerLevel);
    }

    public MinerUpgrade? GetNextUpgrade(ulong userId)
    {
        var inventory = _dataStore.GetUserInventory(userId);
        var nextLevel = inventory.MinerLevel + 1;

        if (nextLevel > _minerUpgrades.Count)
            return null;

        return _minerUpgrades.FirstOrDefault(u => u.Level == nextLevel);
    }
}
