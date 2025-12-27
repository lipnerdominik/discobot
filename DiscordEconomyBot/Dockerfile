# Dockerfile dla Discord Economy Bot
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Kopiuj pliki projektu
COPY *.csproj .
RUN dotnet restore

# Kopiuj resztę plików i zbuduj
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

# Zmienne środowiskowe
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Uruchom bota
ENTRYPOINT ["dotnet", "DiscordEconomyBot.dll"]
