# 🚀 Hosting Discord Economy Bot - Szczegółowy przewodnik

## Spis treści
1. [Railway (Zalecane)](#railway)
2. [Render](#render)
3. [Replit](#replit)
4. [Azure](#azure)
5. [VPS (Własny serwer)](#vps)

---

## Railway

### ✅ Zalety
- Bezpłatne 500h/miesiąc ($5 kredytu)
- Automatyczne deployy z GitHub
- Łatwa konfiguracja
- Wspiera Docker i .NET bezpośrednio

### 📋 Kroki instalacji

1. **Utwórz konto na [Railway](https://railway.app)**

2. **Podłącz repozytorium GitHub**
   - Kliknij "New Project"
   - Wybierz "Deploy from GitHub repo"
   - Autoryzuj Railway do dostępu do GitHub
   - Wybierz repozytorium z botem

3. **Skonfiguruj zmienne środowiskowe**
   - W projekcie przejdź do zakładki "Variables"
   - Dodaj zmienną: `BotToken` = `twoj_token_discord`

4. **Railway wykryje automatycznie .NET**
   - Railway automatycznie wykryje projekt .NET
   - Rozpocznie się build i deploy

5. **Monitoring**
   - W zakładce "Deployments" możesz śledzić logi
   - Bot powinien działać 24/7

### 📝 Railway.toml (opcjonalnie)
```toml
[build]
builder = "NIXPACKS"

[deploy]
startCommand = "dotnet run --project DiscordEconomyBot.csproj"
restartPolicyType = "ON_FAILURE"
```

---

## Render

### ✅ Zalety
- 750h darmowo/miesiąc
- Automatyczne SSL
- Bezpłatne dla projektów open-source

### 📋 Kroki instalacji

1. **Utwórz konto na [Render](https://render.com)**

2. **Nowy Web Service**
   - Dashboard → "New +" → "Web Service"
   - Podłącz repozytorium GitHub

3. **Konfiguracja**
   ```
   Name: discord-economy-bot
   Environment: Docker
   Region: Frankfurt (lub najbliższy)
   Branch: main
   ```

4. **Dockerfile**
   - Render użyje istniejącego Dockerfile
   - Upewnij się, że Dockerfile jest w głównym katalogu

5. **Zmienne środowiskowe**
   - W panelu dodaj:
     - `BotToken` = `twoj_token_discord`

6. **Deploy**
   - Kliknij "Create Web Service"
   - Pierwsze uruchomienie może zająć 5-10 minut

### ⚠️ Uwaga
Render wymaga, aby aplikacja odpowiadała na HTTP requests. Dla bota Discord dodaj endpoint health check:

```csharp
// W Program.cs (opcjonalnie)
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => "OK");

// ... reszta kodu bota
```

---

## Replit

### ✅ Zalety
- Darmowy hosting 24/7
- IDE online
- Łatwy setup

### 📋 Kroki instalacji

1. **Utwórz konto na [Replit](https://replit.com)**

2. **Nowy Repl**
   - Kliknij "+ Create Repl"
   - Wybierz "Import from GitHub"
   - Wklej URL repozytorium

3. **Konfiguracja**
   - Replit automatycznie wykryje .NET
   - W pliku `.replit` upewnij się:
   ```toml
   run = "dotnet run"
   
   [nix]
   channel = "stable-22_11"
   
   [deployment]
   run = ["sh", "-c", "dotnet run"]
   ```

4. **Secrets (zmienne środowiskowe)**
   - W lewym panelu kliknij "Secrets" (ikona kłódki)
   - Dodaj: `BotToken` = `twoj_token_discord`
   
   Odczytaj w kodzie:
   ```csharp
   var botToken = Environment.GetEnvironmentVariable("BotToken");
   ```

5. **Keep Alive**
   - Darmowy Replit "zasypia" po braku aktywności
   - Użyj usługi UptimeRobot lub podobnej do pingowania
   - Dodaj prosty endpoint HTTP (jak w sekcji Render)

6. **Uruchom**
   - Kliknij "Run"
   - Bot będzie działał 24/7

---

## Azure

### ✅ Zalety
- Darmowe 12 miesięcy
- Profesjonalna infrastruktura Microsoft
- $200 kredytu na start

### 📋 Kroki instalacji

1. **Konto Azure**
   - Utwórz konto na [Azure Portal](https://portal.azure.com)
   - Aktywuj darmowy trial

2. **Azure App Service**
   - Przejdź do "App Services"
   - Kliknij "+ Create"
   - Wybierz:
     - Subscription: Free Trial
     - Resource Group: Utwórz nową
     - Name: discord-economy-bot
     - Runtime stack: .NET 8
     - Operating System: Linux
     - Region: West Europe
     - Pricing: Free F1

3. **Deploy z GitHub**
   - W App Service przejdź do "Deployment Center"
   - Source: GitHub
   - Autoryzuj i wybierz repo
   - Branch: main

4. **Zmienne środowiskowe**
   - W App Service → "Configuration"
   - Application settings → "+ New application setting"
   - Name: `BotToken`, Value: `twoj_token_discord`

5. **Continuous Deployment**
   - Azure automatycznie będzie deployować przy każdym push do GitHub

### 💡 Alternatywa: Azure Container Instances
```bash
az container create \
  --resource-group discord-bot-rg \
  --name discord-economy-bot \
  --image yourdockerhub/discord-bot:latest \
  --cpu 1 --memory 1 \
  --environment-variables BotToken=YOUR_TOKEN
```

---

## VPS (Virtual Private Server)

### 🖥️ Popularne dostawcy VPS
- **Oracle Cloud** - darmowy tier (1 GB RAM)
- **Google Cloud** - $300 kredytu na 90 dni
- **DigitalOcean** - $200 kredytu na 60 dni (dla studentów)
- **Vultr** - od $3.50/miesiąc
- **Hetzner** - od €4.15/miesiąc

### 📋 Instalacja na Ubuntu/Debian

1. **Połącz się z serwerem**
   ```bash
   ssh user@your-server-ip
   ```

2. **Zainstaluj .NET 8**
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 8.0
   
   # Dodaj do PATH
   echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
   echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
   source ~/.bashrc
   ```

3. **Sklonuj repozytorium**
   ```bash
   git clone https://github.com/your-username/discord-economy-bot.git
   cd discord-economy-bot/DiscordEconomyBot
   ```

4. **Skonfiguruj appsettings.json**
   ```bash
   nano appsettings.json
   # Wpisz swój token
   ```

5. **Zbuduj i uruchom**
   ```bash
   dotnet build
   dotnet run
   ```

6. **Uruchom jako usługę systemd**
   
   Utwórz plik `/etc/systemd/system/discord-bot.service`:
   ```ini
   [Unit]
   Description=Discord Economy Bot
   After=network.target

   [Service]
   Type=notify
   User=yourusername
   WorkingDirectory=/home/yourusername/discord-economy-bot/DiscordEconomyBot
   ExecStart=/home/yourusername/.dotnet/dotnet run
   Restart=always
   RestartSec=10

   [Install]
   WantedBy=multi-user.target
   ```

   Uruchom usługę:
   ```bash
   sudo systemctl enable discord-bot
   sudo systemctl start discord-bot
   sudo systemctl status discord-bot
   ```

7. **Logi**
   ```bash
   sudo journalctl -u discord-bot -f
   ```

---

## 🔧 Porównanie platform

| Platforma | Koszt | Czas działania | Łatwość | Zalety |
|-----------|-------|----------------|---------|--------|
| **Railway** | Darmowe 500h | 24/7 | ⭐⭐⭐⭐⭐ | Najłatwiejszy setup |
| **Render** | Darmowe 750h | 24/7 | ⭐⭐⭐⭐ | Dobre dla projektów open-source |
| **Replit** | Darmowe | 24/7* | ⭐⭐⭐⭐⭐ | IDE online, wymaga keep-alive |
| **Azure** | 12m darmowe | 24/7 | ⭐⭐⭐ | Profesjonalne, skalowalne |
| **VPS** | Od $3.50/m | 24/7 | ⭐⭐ | Pełna kontrola |

*Replit wymaga usługi keep-alive dla darmowego planu

---

## 🎯 Polecany wybór

### Dla początkujących: **Railway**
- Zero konfiguracji
- Automatyczne deploye
- Dobre logi

### Dla średnio zaawansowanych: **Render**
- Dłuższy czas działania
- Dobre dla projektów rozwijających się

### Dla zaawansowanych: **VPS (Oracle Cloud/Hetzner)**
- Pełna kontrola
- Możliwość hostowania wielu botów
- Nauka administracji serwerem

---

## 🆘 Troubleshooting

### Bot się nie uruchamia
- Sprawdź logi
- Upewnij się, że token jest prawidłowy
- Zweryfikuj intents w Discord Developer Portal

### Bot "zasypia" (Replit)
- Użyj UptimeRobot do pingowania endpointu `/health`
- Rozważ upgrade do płatnego planu

### Brak pamięci
- Zmniejsz cachowanie
- Użyj bazy danych zamiast plików JSON (dla dużych serwerów)

### Błędy kompilacji
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📚 Dodatkowe zasoby

- [Discord.Net Dokumentacja](https://discordnet.dev/)
- [Railway Docs](https://docs.railway.app/)
- [Render Docs](https://render.com/docs)
- [Azure Docs](https://docs.microsoft.com/azure/)

---

Powodzenia z hostingiem! 🚀
