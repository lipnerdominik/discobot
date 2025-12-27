# Zadanie: Bot Discord z systemem ekonomii (C#)

## Cel
Stwórz bota na Discorda, który:
- Przyznaje użytkownikom monety za aktywność (np. pisanie wiadomości, przebywanie na czacie głosowym, zapraszanie nowych użytkowników, udzielanie się w ankietach, uczestnictwo w konkursach i wydarzeniach, bonus za daily, osiągnięcia – np. określona ilość wiadomości lub dni aktywności).
- Pozwala wymieniać monety na rangi lub inne nagrody.
- Umożliwia adminom zarządzanie ekonomią (resetowanie, dodawanie monet itp.).

## Wymagania funkcjonalne
1. **System monet** – użytkownicy zdobywają monety za aktywność:
    - Pisanie wiadomości
    - Przebywanie na czacie głosowym
    - Zapraszanie nowych użytkowników
    - Udzielanie się w ankietach
    - Uczestnictwo w konkursach i wydarzeniach
    - Bonus za codzienną aktywność ("daily")
    - Zdobywanie osiągnięć (np. określona liczba wiadomości, dni aktywności)
2. **Sklep z rangami** – użytkownicy mogą wydawać monety na rangi.
3. **Komendy** – bot obsługuje komendy ekonomiczne (`!saldo`, `!kup`, `!sklep`, `!top`, `!dodajmonety`).
4. **Panel admina** – komendy tylko dla adminów do zarządzania ekonomią.
5. **Trwałość danych** – dane użytkowników i ekonomii są zapisywane (np. plik JSON lub baza danych).
6. **Konfigurowalność** – możliwość łatwej zmiany ustawień (np. nagrody za wiadomość, ceny rang).

## Wymagania techniczne
- Język: **C#**
- Biblioteka: [Discord.Net](https://discordnet.dev/)
- Przechowywanie danych: plik JSON (na początek)
- Możliwość łatwego hostowania (np. Railway, Replit, Azure – darmowy tier)

## Proponowana struktura projektu
- `Program.cs` – uruchamianie bota
- `Services/` – logika ekonomii, sklep, aktywność
- `Commands/` – obsługa komend
- `Models/` – modele danych (użytkownik, transakcja, ranga)
- `Data/` – zapisywanie/odczyt danych
- `Config/` – pliki konfiguracyjne

## Etapy realizacji
1. **Inicjalizacja projektu** – utworzenie projektu C# i instalacja Discord.Net.
2. **Podstawowy bot** – logowanie i reagowanie na komendy.
3. **System ekonomii** – przyznawanie monet za aktywność.
4. **Komendy ekonomiczne** – saldo, topka, sklep, kupowanie rang.
5. **Panel admina** – komendy dla adminów.
6. **Trwałość danych** – zapis i odczyt danych użytkowników.
7. **Testy** – podstawowe testy jednostkowe.
8. **Instrukcja uruchomienia i hostingu** – opis jak uruchomić i hostować bota za darmo.

## Dodatkowe uwagi
- Kod powinien być czytelny i dobrze udokumentowany.
- Projekt powinien być łatwy do rozbudowy w przyszłości (np. o nowe nagrody, integracje).

---

**Wykonaj powyższe zadanie krok po kroku.**