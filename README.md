# SafeBox Password Manager

SafeBox Password Manager to aplikacja desktopowa napisana w C#/.NET z interfejsem WPF. Projekt powstał na potrzeby przedmiotu Programowanie obiektowe.

## Funkcje aplikacji

- logowanie hasłem głównym,
- weryfikacja 2FA zgodna z Google Authenticator,
- dodawanie, edycja i usuwanie wpisów z hasłami,
- wyszukiwanie zapisanych wpisów,
- generator silnych haseł,
- zapis danych do pliku JSON,
- szyfrowanie danych przed zapisem do pliku.

## Struktura projektu

- `SafeBoxPasswordManager/MainWindow.xaml` - interfejs graficzny WPF,
- `SafeBoxPasswordManager/MainWindow.xaml.cs` - logika głównego okna,
- `SafeBoxPasswordManager/Models/PasswordEntry.cs` - model pojedynczego wpisu,
- `SafeBoxPasswordManager/Services/MasterPasswordService.cs` - obsługa hasła głównego,
- `SafeBoxPasswordManager/Services/PasswordStorageService.cs` - zapis i odczyt wpisów,
- `SafeBoxPasswordManager/Services/EncryptionService.cs` - szyfrowanie danych,
- `SafeBoxPasswordManager/Services/TotpService.cs` - obsługa kodów 2FA.

## Uruchomienie projektu

1. Otwórz plik `SafeBoxPasswordManager.slnx` w Visual Studio.
2. Wybierz konfigurację `Debug` oraz `Any CPU`.
3. Uruchom projekt zielonym przyciskiem Start.
4. Przy pierwszym uruchomieniu ustaw hasło główne i skonfiguruj Google Authenticator.

## Dokumentacja

Raport końcowy znajduje się w folderze `docs`.

## Ważne

Do repozytorium nie są dodawane prywatne pliki danych użytkownika, takie jak:

- `password_entries.json`,
- `master_password.dat`,
- `totp_secret.dat`,
- foldery `bin`, `obj`, `.vs`.

Pliki te powstają lokalnie podczas działania aplikacji.
