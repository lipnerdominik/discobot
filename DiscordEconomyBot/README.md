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
- ⛏️ **Kopanie surowców** - 5-500 monet (cooldown 60s) ⚠️ 15% szans na atak goblina!
- ✨ **Podwójny drop z górnika** - do 1000 monet! (2 przedmioty naraz)
- 🎲 **Gra w kubki** - wygraj x2 zakładu! (10-1000 monet)
- 🎤 **Przebywanie na voice** - 2 monety/minutę
- 👥 **Zapraszanie użytkowników** - 100 monet
- 📊 **Udział w ankietach** - 10 monet
- 🎉 **Uczestnictwo w wydarzeniach** - 50 monet
- 🎁 **Codzienna nagroda** - 100 monet
- 🏆 **Osiągnięcia** - 50-1000 monet

### ⚠️ Uwaga przy kopaniu!
Podczas kopania masz **15% szans** na napotkanie goblina, który ukradnie Ci **70-150 monet**! Upewnij się, że masz wystarczająco monet zanim zaczniesz kopać.

### 👷 System Górnika
Możesz ulepszać swojego górnika, aby zwiększyć szansę na **podwójny drop** podczas kopania! 

**Poziomy górnika:**
- **Poziom 1** (500 monet) - 10% szans na podwójny drop
- **Poziom 2** (1,500 monet) - 20% szans na podwójny drop
- **Poziom 3** (3,500 monet) - 35% szans na podwójny drop
- **Poziom 4** (7,500 monet) - 50% szans na podwójny drop
- **Poziom 5** (15,000 monet) - 70% szans na podwójny drop

Podwójny drop oznacza, że wykopiesz **2 losowe przedmioty** zamiast jednego! Możesz dostać np. 2x żelazo, lub żelazo + kamień, albo nawet 2x diament! 💎💎

### 🎲 Gra w Kubki (Shell Game)
Klasyczna gra hazardowa! Pod jednym z trzech kubków ukryta jest kulka :red_circle:

**Zasady:**
1. Postaw zakład od **10 do 1000 monet** używając `/kubki <zakład>`
2. Bot ukryje kulkę pod jednym z trzech kubków i wyświetli **3 przyciski**
3. **Kliknij przycisk** z numerem kubka (1️⃣, 2️⃣ lub 3️⃣)
4. Jeśli zgadniesz, **wygrywasz x2 zakładu**! 🎉
5. Jeśli nie trafisz, tracisz zakład 😢

**Przykłady:**
- Zakład: 100 monet → Wygrana: 200 monet (+100 zysku)
- Zakład: 500 monet → Wygrana: 1000 monet (+500 zysku)
- Maksymalny zakład: 1000 monet → Możliwa wygrana: 2000 monet! 💰

⏱️ Masz **5 minut** na wybór kubka, w przeciwnym razie sesja wygasa.

✨ **Nowość:** Gra używa interaktywnych przycisków Discord - wystarczy kliknąć!

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
  "BotToken": "YOUR_BOT_TOKEN_HERE",
  "Urls": "http://0.0.0.0:5000"
}
```

4. **Uruchom bota**
```bash
dotnet run
```

## 🏥 Health Endpoint

Bot udostępnia prosty HTTP endpoint do sprawdzania statusu i wersji:

### Dostępne endpointy:

**`GET /health`** - Sprawdź status bota
```json
{
  "status": "Healthy",
  "version": "1.0.0-a1b2c3d4",
  "commitHash": "a1b2c3d4",
  "botUsername": "YourBotName#1234",
  "guildCount": 5,
  "isConnected": true,
  "startTime": "2024-01-15T10:30:00Z",
  "uptime": "2.15:30:45"
}
```

**`GET /`** - Informacje o bocie
```json
{
  "name": "Discord Economy Bot",
  "version": "1.0.0-a1b2c3d4",
  "commitHash": "a1b2c3d4",
  "status": "Running",
  "endpoints": [
    "/health - Bot health and status information"
  ]
}
```

### Użycie:

```bash
# Lokalne sprawdzenie
curl http://localhost:5000/health

# Sprawdzenie na serwerze
curl http://your-server-ip:5000/health

# Sprawdź tylko wersję (z jq)
curl -s http://your-server-ip:5000/health | jq -r '.version'

# Sprawdź commit hash
curl -s http://your-server-ip:5000/health | jq -r '.commitHash'
```

### Format wersji:

Bot automatycznie wykrywa Git commit hash i dodaje go do wersji:
- **Z Git:** `1.0.0-a1b2c3d4` (ostatnie 8 znaków commit hash)
- **Bez Git:** `1.0.0-unknown` (jeśli brak pliku version.txt)

Commit hash jest zapisywany w pliku `version.txt` podczas procesu deploymentu przez skrypt `deploy.ps1`.

### Proces deploymentu z weryfikacją wersji:

Skrypt `deploy.ps1` automatycznie:
1. ✅ Sprawdza czy jesteś w repozytorium Git
2. ✅ Pobiera aktualny commit hash (8 znaków)
3. ⚠️ **Ostrzega** jeśli masz niezcommitowane zmiany
4. ⚠️ **Ostrzega** jeśli ta sama wersja jest już wdrożona
5. 📝 Tworzy plik `version.txt` z commit hash
6. 🔨 Buduje projekt (commit hash jest wbudowany w build)
7. 📦 Uploaduje pliki na serwer
8. 🏥 Weryfikuje czy wdrożona wersja jest poprawna

### Konfiguracja portu:

Domyślnie bot nasłuchuje na porcie **5000**. Możesz zmienić to w `appsettings.json`:

```json
{
  "Urls": "http://0.0.0.0:8080"
}
```

Lub przez zmienne środowiskowe:
```bash
export ASPNETCORE_URLS="http://0.0.0.0:8080"
```

### Monitorowanie deploymentu:

Health endpoint jest idealny do:
- ✅ Weryfikacji czy nowa wersja została wdrożona
- ✅ Sprawdzenia czy bot jest online i połączony z Discord
- ✅ Monitorowania uptime'u
- ✅ Automatycznych health checków w systemach monitorujących (UptimeRobot, Healthchecks.io)

## 🎮 Komendy użytkownika (Slash Commands)

| Komenda | Opis |
|---------|------|
| `/saldo` | Sprawdź swoje saldo monet |
| `/daily` | Odbierz codzienną nagrodę |
| `/top` | Zobacz ranking najbogatszych |
| `/sklep` | Zobacz dostępne rangi |
| `/kup @Ranga` | Kup rangę ze sklepu (mention rangi) |
| `/kop` | Wykop surowce (cooldown: 60s) |
| `/ekwipunek` | Zobacz swój ekwipunek i przedmioty |
| `/surowce` | Zobacz listę wszystkich surowców |
| `/sprzedaj <przedmiot> <ilość>` | Sprzedaj przedmiot z ekwipunku |
| `/sprzedajwszystko` | Sprzedaj wszystkie przedmioty |
| `/gornik` | Zobacz informacje o ulepszeniach górnika |
| `/ulepszgornika` | Ulepsz górnika (zwiększa szansę na podwójny drop!) |
| `/kubki <zakład>` | Zagraj w grę Shell Game - kliknij przycisk aby wybrać kubek! |
| `/statystyki` | Zobacz swoje statystyki |
| `/osiagniecia` | Zobacz swoje osiągnięcia |
| `/pomoc` | Wyświetl listę komend |

## 🔧 Komendy administratora (Slash Commands)

| Komenda | Opis |
|---------|------|
| `/admin dodajmonety @użytkownik <ilość>` | Dodaj monety użytkownikowi |
| `/admin usunmonety @użytkownik <ilość>` | Usuń monety użytkownikowi |
| `/admin dodajrange @Ranga <cena> [opis]` | Dodaj rangę do sklepu (mention rangi) |
| `/admin usunrange @Ranga` | Usuń rangę ze sklepu (mention rangi) |
| `/admin wydarzenie @użytkownicy` | Przyznaj nagrody za wydarzenie (do 5 osób) |
| `/admin pomoc` | Wyświetl listę komend admina |

**Uwaga:** Komendy administratorskie są dostępne tylko dla użytkowników z uprawnieniami **Administrator** na serwerze.

## 🎮 Komendy tekstowe (Legacy - opcjonalne)

Bot nadal wspiera tradycyjne komendy tekstowe z prefiksem `!`:

**Użytkownik:**
- `!saldo`, `!daily`, `!top`, `!sklep`, `!kup <ID>`, `!kop`, `!ekwipunek`, `!surowce`, `!sprzedaj <przedmiot> <ilość>`, `!sprzedajwszystko`, `!statystyki`, `!osiagniecia`, `!pomoc`

**Administrator:**
- `!admin dodajmonety <@user> <ilość>`
- `!admin usunmonety <@user> <ilość>`
- `!admin dodajrange <ID> <cena> <opis>`
- `!admin usunrange <ID>`
- `!admin wydarzenie <@users>`
- `!admin pomoc`

**Uwaga:** Komendy tekstowe nadal używają ID zamiast mention rangi. Zalecamy używanie komend slash dla lepszego doświadczenia.

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
│   ├── SlashCommands.cs      # Komendy slash (nowoczesne)
│   ├── EconomyCommands.cs    # Komendy ekonomiczne (legacy)
│   └── AdminCommands.cs      # Komendy administracyjne (legacy)
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
- `inventories.json` - ekwipunki użytkowników (wykopane surowce)

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
2. Scopes: **`bot`** oraz **`applications.commands`** (wymagane dla slash commands)
3. Bot Permissions (minimum do działania ekonomii i sklepu):
   - Manage Roles
   - Read Messages/View Channels
   - Send Messages
   - Embed Links
   - Read Message History
   - Add Reactions
4. Skopiuj wygenerowany URL i otwórz w przeglądarce

**Ważne:** Bot używa slash commands, więc **musisz** zaznaczyć scope `applications.commands` podczas generowania linku zaproszenia!

## 🛠️ Rozbudowa

Bot jest zaprojektowany z myślą o łatwej rozbudowie:
- Dodaj nowe źródła monet w `EconomyService`
- Stwórz nowe komendy w `Commands/SlashCommands.cs`
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
5. Upewnij się, że w **OAuth2 URL Generator** zaznaczono zarówno `bot` jak i `applications.commands`
6. Jeśli komendy nie pojawiają się, zrestartuj bota - slash commands rejestrują się przy starcie

## 🎉 Miłego używania!

Stworzono z ❤️ przy użyciu Discord.Net
