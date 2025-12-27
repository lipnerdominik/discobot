using Discord.WebSocket;

namespace DiscordEconomyBot.Services;

public class VoiceTrackingService
{
    private readonly Dictionary<ulong, DateTime> _voiceJoinTimes = new();
    private readonly EconomyService _economyService;

    public VoiceTrackingService(EconomyService economyService)
    {
        _economyService = economyService;
    }

    public void UserJoinedVoice(ulong userId)
    {
        _voiceJoinTimes[userId] = DateTime.UtcNow;
    }

    public void UserLeftVoice(ulong userId)
    {
        if (_voiceJoinTimes.TryGetValue(userId, out var joinTime))
        {
            var duration = DateTime.UtcNow - joinTime;
            if (duration.TotalMinutes >= 1) // Minimum 1 minuta
            {
                _economyService.HandleVoiceTime(userId, duration);
            }
            _voiceJoinTimes.Remove(userId);
        }
    }

    public void UserMovedVoice(ulong userId, SocketVoiceChannel? before, SocketVoiceChannel? after)
    {
        // Jeśli użytkownik opuścił kanał
        if (before != null && after == null)
        {
            UserLeftVoice(userId);
        }
        // Jeśli użytkownik dołączył do kanału
        else if (before == null && after != null)
        {
            UserJoinedVoice(userId);
        }
        // Jeśli użytkownik przeniósł się między kanałami (kontynuuj śledzenie)
    }
}
