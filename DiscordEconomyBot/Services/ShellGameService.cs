using DiscordEconomyBot.Models;

namespace DiscordEconomyBot.Services;

public class ShellGameService
{
    private readonly EconomyService _economyService;
    private readonly Dictionary<ulong, ShellGameSession> _activeSessions = new();
    private readonly Random _random = new();

    public ShellGameService(EconomyService economyService)
    {
        _economyService = economyService;
    }

    public (bool success, string message, int correctCup) StartGame(ulong userId, long betAmount)
    {
        // SprawdŸ czy gracz ma aktywn¹ sesjê
        if (_activeSessions.ContainsKey(userId))
        {
            return (false, "Masz ju¿ aktywn¹ grê w kubki! Wybierz kubek lub poczekaj na wygaœniêcie sesji.", 0);
        }

        // SprawdŸ minimalny zak³ad
        if (betAmount < 10)
        {
            return (false, "Minimalny zak³ad to **10 monet**!", 0);
        }

        // SprawdŸ maksymalny zak³ad
        if (betAmount > 1000)
        {
            return (false, "Maksymalny zak³ad to **1000 monet**!", 0);
        }

        // SprawdŸ saldo gracza
        var balance = _economyService.GetBalance(userId);
        if (balance < betAmount)
        {
            return (false, $"Nie masz wystarczaj¹co monet! Twoje saldo: **{balance}** monet.", 0);
        }

        // Usuñ monety gracza (zak³ad)
        _economyService.RemoveCoins(userId, betAmount, "Zak³ad w grze kubki");

        // Losuj kubek z kulk¹ (1-3)
        var correctCup = _random.Next(1, 4);

        // Utwórz sesjê gry
        _activeSessions[userId] = new ShellGameSession
        {
            UserId = userId,
            CorrectCup = correctCup,
            BetAmount = betAmount,
            CreatedAt = DateTime.UtcNow
        };

        return (true, $"Gra rozpoczêta! Postawi³eœ **{betAmount}** monet.\nWybierz kubek (1, 2 lub 3) w którym jest kulka! :red_circle:", correctCup);
    }

    public (bool exists, bool won, string message, long winAmount) GuessAndEnd(ulong userId, int guessedCup)
    {
        // SprawdŸ czy gracz ma aktywn¹ sesjê
        if (!_activeSessions.TryGetValue(userId, out var session))
        {
            return (false, false, "Nie masz aktywnej gry w kubki! U¿yj `/kubki <zak³ad>` aby rozpocz¹æ grê.", 0);
        }

        // Usuñ sesjê (gra siê koñczy)
        _activeSessions.Remove(userId);

        // SprawdŸ czy sesja nie wygas³a (5 minut)
        if ((DateTime.UtcNow - session.CreatedAt).TotalMinutes > 5)
        {
            return (true, false, "Twoja sesja wygas³a! Spróbuj ponownie.", 0);
        }

        // SprawdŸ czy gracz wybra³ prawid³owy kubek
        if (guessedCup == session.CorrectCup)
        {
            // Wygrana - zwróæ zak³ad x2
            var winAmount = session.BetAmount * 2;
            _economyService.AddCoins(userId, winAmount, "Wygrana w grze kubki");
            
            return (true, true, 
                $":trophy: **WYGRANA!** Kulka by³a pod kubkiem **{session.CorrectCup}**!\n" +
                $"Wygra³eœ **{winAmount}** monet! (+{session.BetAmount} zysku)", 
                winAmount);
        }
        else
        {
            // Przegrana
            return (true, false, 
                $":x: **PRZEGRANA!** Kulka by³a pod kubkiem **{session.CorrectCup}**, nie pod **{guessedCup}**.\n" +
                $"Straci³eœ **{session.BetAmount}** monet.", 
                0);
        }
    }

    public bool HasActiveSession(ulong userId)
    {
        return _activeSessions.ContainsKey(userId);
    }

    public void CancelSession(ulong userId)
    {
        if (_activeSessions.TryGetValue(userId, out var session))
        {
            _activeSessions.Remove(userId);
            // Zwróæ zak³ad
            _economyService.AddCoins(userId, session.BetAmount, "Anulowanie gry kubki");
        }
    }

    // Czyszczenie wygas³ych sesji
    public void CleanupExpiredSessions()
    {
        var expiredSessions = _activeSessions
            .Where(kvp => (DateTime.UtcNow - kvp.Value.CreatedAt).TotalMinutes > 5)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var userId in expiredSessions)
        {
            _activeSessions.Remove(userId);
        }
    }
}
