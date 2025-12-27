# Discord Economy Bot 🤖💰

Bot Discord z systemem ekonomii napisany w C# przy użyciu Discord.Net.

## ✨ Funkcje

- 💰 **System monet** - użytkownicy zarabiają monety za różne aktywności
- 🎭 **Sklep z rangami** - możliwość kupowania rang za monety
- 🏆 **System osiągnięć** - nagrody za kamienie milowe
- 📊 **Statystyki** - szczegółowe statystyki użytkowników
- 🎁 **Codzienna nagroda** - bonus za codzienną aktywność
- 👑 **Panel admina** - zarządzanie ekonomią serwera

## 💎 Sposoby zarabiania monet

- 📝 **Pisanie wiadomości** - 5 monet (cooldown 30s)
- 🎤 **Przebywanie na voice** - 2 monety/minutę
- 👥 **Zapraszanie użytkowników** - 100 monet
- 📊 **Udział w ankietach** - 10 monet
- 🎉 **Uczestnictwo w wydarzeniach** - 50 monet
- 🎁 **Codzienna nagroda** - 100 monet
- 🏆 **Osiągnięcia** - 50-1000 monet

## 📋 Wymagania

- .NET 8.0 lub nowszy
- Konto Discord Bot (token z [Discord Developer Portal](https://discord.com/developers/applications))

## 🚀 Instalacja

1. **Sklonuj/pobierz projekt**

2. **Zainstaluj zależności**
```bash
cd DiscordEconomyBot
dotnet restore
```

3. **Skonfiguruj bota**
   
   Edytuj plik `appsettings.json` i wpisz token bota:
```json
{
  "BotToken": "YOUR_BOT_TOKEN_HERE"
}
```

4. **Uruchom bota**
```bash
dotnet run
```

## 🎮 Komendy użytkownika

| Komenda | Opis |
|---------|------|
| `!saldo` | Sprawdź swoje saldo monet |
| `!daily` | Odbierz codzienną nagrodę |
| `!top` | Zobacz ranking najbogatszych |
| `!sklep` | Zobacz dostępne rangi |
| `!kup <ID>` | Kup rangę ze sklepu |
| `!statystyki` | Zobacz swoje statystyki |
| `!osiagniecia` | Zobacz swoje osiągnięcia |
| `!pomoc` | Wyświetl listę komend |

## 🔧 Komendy administratora

| Komenda | Opis |
|---------|------|
| `!admin dodajmonety <@user> <ilość>` | Dodaj monety użytkownikowi |
| `!admin usunmonety <@user> <ilość>` | Usuń monety użytkownikowi |
| `!admin dodajrange <ID> <cena> <opis>` | Dodaj rangę do sklepu |
| `!admin usunrange <ID>` | Usuń rangę ze sklepu |
| `!admin wydarzenie <@users>` | Przyznaj nagrody za wydarzenie |
| `!admin pomoc` | Wyświetl listę komend admina |

## ⚙️ Konfiguracja

Edytuj `appsettings.json` aby dostosować ustawienia ekonomii:

```json
{
  "EconomyConfig": {
    "CoinsPerMessage": 5,
    "MessageCooldownSeconds": 30,
    "CoinsPerVoiceMinute": 2,
    "CoinsPerInvite": 100,
    "CoinsPerPoll": 10,
    "CoinsPerEvent": 50,
    "DailyReward": 100,
    "Achievements": {
      "100_messages": 50,
      "500_messages": 200,
      "1000_messages": 500,
      "7_days_active": 150,
      "30_days_active": 1000
    }
  }
}
```

## 📁 Struktura projektu

```
DiscordEconomyBot/
├── Bot/
│   └── BotClient.cs          # Główny klient bota
├── Commands/
│   ├── EconomyCommands.cs    # Komendy ekonomiczne
│   └── AdminCommands.cs      # Komendy administracyjne
├── Services/
│   ├── EconomyService.cs     # Logika ekonomii
│   ├── RoleShopService.cs    # Sklep z rangami
│   └── VoiceTrackingService.cs # Śledzenie czasu na voice
├── Models/
│   ├── UserBalance.cs        # Model użytkownika
│   ├── ShopRole.cs          # Model rangi
│   ├── Transaction.cs       # Model transakcji
│   └── EconomyConfig.cs     # Konfiguracja ekonomii
├── Data/
│   └── JsonDataStore.cs     # Przechowywanie danych
├── appsettings.json         # Konfiguracja
└── Program.cs              # Punkt wejścia
```

## 💾 Przechowywanie danych

Bot zapisuje dane w plikach JSON w folderze `data/`:
- `users.json` - salda i statystyki użytkowników
- `roles.json` - rangi dostępne w sklepie
- `transactions.json` - historia transakcji

## ☁️ Darmowy hosting

Bot można hostować za darmo na różnych platformach:

### 1. **Railway** (Zalecane)
- Darmowy plan: 500h/miesiąc
- Łatwe wdrożenie z GitHub
- [railway.app](https://railway.app)

**Kroki:**
1. Utwórz konto na Railway
2. Kliknij "New Project" → "Deploy from GitHub"
3. Wybierz repozytorium
4. Ustaw zmienne środowiskowe (BotToken)
5. Deploy!

### 2. **Replit**
- Darmowy hosting 24/7 (z Keep Alive)
- IDE online
- [replit.com](https://replit.com)

### 3. **Render**
- 750h darmowo/miesiąc
- [render.com](https://render.com)

### 4. **Heroku** (z limitami)
- Darmowy plan z ograniczeniami
- [heroku.com](https://heroku.com)

### 5. **Azure Free Tier**
- Darmowe 12 miesięcy
- [azure.microsoft.com](https://azure.microsoft.com)

## 🔑 Jak uzyskać Token Discord Bot

1. Przejdź do [Discord Developer Portal](https://discord.com/developers/applications)
2. Kliknij "New Application"
3. Nazwij aplikację
4. Przejdź do zakładki "Bot"
5. Kliknij "Add Bot"
6. W sekcji "Token" kliknij "Copy"
7. Wklej token do `appsettings.json`

**Ważne:** Pamiętaj włączyć w Bot Settings:
- ✅ Presence Intent
- ✅ Server Members Intent
- ✅ Message Content Intent

## 🔗 Zaproszenie bota na serwer

Wygeneruj link zaproszenia w Developer Portal:
1. Zakładka "OAuth2" → "URL Generator"
2. Scopes: `bot` oraz (opcjonalnie) `applications.commands` jeśli chcesz slash-komendy
3. Bot Permissions (minimum do działania ekonomii i sklepu):
   - Manage Roles
   - Read Messages/View Channels
   - Send Messages
   - Embed Links
   - Read Message History
   - Add Reactions
4. Skopiuj wygenerowany URL i otwórz w przeglądarce

Uwaga: Nie używaj opcji "User Install" ani zakresów dla instalacji użytkownika. Ten bot jest typem `bot` i wymaga zaproszenia na serwer z zakresem `bot`. Wybranie niewłaściwych zakresów powoduje błąd o nieodpowiednich zakresach.

## 🛠️ Rozbudowa

Bot jest zaprojektowany z myślą o łatwej rozbudowie:
- Dodaj nowe źródła monet w `EconomyService`
- Stwórz nowe komendy w `Commands/`
- Dodaj nowe modele w `Models/`
- Rozbuduj system osiągnięć w `EconomyConfig`

## 📝 Licencja

Projekt open-source - możesz go swobodnie modyfikować i używać.

## 🤝 Wsparcie

Jeśli masz pytania lub problemy:
1. Sprawdź logi w konsoli
2. Upewnij się, że bot ma odpowiednie uprawnienia
3. Sprawdź czy token jest prawidłowy
4. Zweryfikuj czy włączone są Intents w Developer Portal
5. Jeśli pojawia się błąd z zakresami, upewnij się, że w **OAuth2 URL Generator** wybrano tylko `bot` (i ewentualnie `applications.commands`) — bez zakresów instalacji użytkownika.

## 🎉 Miłego używania!

Stworzono z ❤️ przy użyciu Discord.Net
