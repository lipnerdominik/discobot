using DiscordEconomyBot.Data;
using DiscordEconomyBot.Models;

namespace DiscordEconomyBot.Services;

public class EconomyService
{
    private readonly JsonDataStore _dataStore;
    private readonly EconomyConfig _config;
    private readonly Dictionary<ulong, DateTime> _messageCooldowns = new();

    public EconomyService(JsonDataStore dataStore, EconomyConfig config)
    {
        _dataStore = dataStore;
        _config = config;
    }

    public long GetBalance(ulong userId)
    {
        return _dataStore.GetUser(userId).Balance;
    }

    public bool AddCoins(ulong userId, long amount, string reason)
    {
        var user = _dataStore.GetUser(userId);
        user.Balance += amount;
        _dataStore.UpdateUser(user);

        _dataStore.AddTransaction(new Transaction
        {
            UserId = userId,
            Amount = amount,
            Type = "earn",
            Description = reason,
            Timestamp = DateTime.UtcNow
        });

        return true;
    }

    public bool RemoveCoins(ulong userId, long amount, string reason)
    {
        var user = _dataStore.GetUser(userId);
        if (user.Balance < amount)
            return false;

        user.Balance -= amount;
        _dataStore.UpdateUser(user);

        _dataStore.AddTransaction(new Transaction
        {
            UserId = userId,
            Amount = -amount,
            Type = "spend",
            Description = reason,
            Timestamp = DateTime.UtcNow
        });

        return true;
    }

    public void HandleMessageSent(ulong userId, string username)
    {
        if (_messageCooldowns.ContainsKey(userId))
        {
            var timeSinceLastMessage = DateTime.UtcNow - _messageCooldowns[userId];
            if (timeSinceLastMessage.TotalSeconds < _config.MessageCooldownSeconds)
                return;
        }

        _messageCooldowns[userId] = DateTime.UtcNow;

        var user = _dataStore.GetUser(userId);
        user.Username = username;
        user.MessageCount++;
        user.LastActivity = DateTime.UtcNow;

        AddCoins(userId, _config.CoinsPerMessage, "Wiadomość na czacie");
        CheckMessageAchievements(user);
    }

    public void HandleVoiceTime(ulong userId, TimeSpan duration)
    {
        var user = _dataStore.GetUser(userId);
        user.VoiceTime += duration;
        _dataStore.UpdateUser(user);

        int minutes = (int)duration.TotalMinutes;
        if (minutes > 0)
        {
            AddCoins(userId, minutes * _config.CoinsPerVoiceMinute, $"Czas na czacie głosowym: {minutes} min");
        }
    }

    public void HandleInvite(ulong userId)
    {
        var user = _dataStore.GetUser(userId);
        user.InviteCount++;
        _dataStore.UpdateUser(user);

        AddCoins(userId, _config.CoinsPerInvite, "Zaproszenie nowego użytkownika");
    }

    public void HandlePollParticipation(ulong userId)
    {
        var user = _dataStore.GetUser(userId);
        user.PollParticipation++;
        _dataStore.UpdateUser(user);

        AddCoins(userId, _config.CoinsPerPoll, "Udział w ankiecie");
    }

    public void HandleEventParticipation(ulong userId)
    {
        var user = _dataStore.GetUser(userId);
        user.EventParticipation++;
        _dataStore.UpdateUser(user);

        AddCoins(userId, _config.CoinsPerEvent, "Udział w wydarzeniu");
    }

    public (bool success, string message) ClaimDaily(ulong userId)
    {
        var user = _dataStore.GetUser(userId);
        var now = DateTime.UtcNow;

        if (user.LastDaily.Date == now.Date)
        {
            var timeUntilNext = user.LastDaily.AddDays(1) - now;
            return (false, $"Już odebrałeś dzisiejszą nagrodę! Następna za: {timeUntilNext.Hours}h {timeUntilNext.Minutes}m");
        }

        user.LastDaily = now;
        user.DaysActive++;
        _dataStore.UpdateUser(user);

        AddCoins(userId, _config.DailyReward, "Codzienna nagroda");
        CheckDaysActiveAchievements(user);

        return (true, $"Otrzymałeś {_config.DailyReward} monet! 🎁");
    }

    private void CheckMessageAchievements(UserBalance user)
    {
        CheckAchievement(user, "100_messages", user.MessageCount >= 100, "Wysłano 100 wiadomości! 📝");
        CheckAchievement(user, "500_messages", user.MessageCount >= 500, "Wysłano 500 wiadomości! 📝✨");
        CheckAchievement(user, "1000_messages", user.MessageCount >= 1000, "Wysłano 1000 wiadomości! 📝🌟");
    }

    private void CheckDaysActiveAchievements(UserBalance user)
    {
        CheckAchievement(user, "7_days_active", user.DaysActive >= 7, "7 dni aktywności! 🗓️");
        CheckAchievement(user, "30_days_active", user.DaysActive >= 30, "30 dni aktywności! 🗓️🌟");
    }

    private void CheckAchievement(UserBalance user, string achievementKey, bool condition, string description)
    {
        if (condition && !user.Achievements.Contains(achievementKey))
        {
            user.Achievements.Add(achievementKey);
            _dataStore.UpdateUser(user);

            if (_config.Achievements.ContainsKey(achievementKey))
            {
                AddCoins(user.UserId, _config.Achievements[achievementKey], $"Osiągnięcie: {description}");
            }
        }
    }

    public List<UserBalance> GetTopUsers(int count = 10)
    {
        return _dataStore.GetAllUsers()
            .OrderByDescending(u => u.Balance)
            .Take(count)
            .ToList();
    }

    public UserBalance GetUserStats(ulong userId)
    {
        return _dataStore.GetUser(userId);
    }
}
