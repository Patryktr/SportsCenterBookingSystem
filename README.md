# SportsCenterBookingSystem

### Wprowadzenie i cel projektu
Celem projektu było stworzenie kompleksowego systemu API przeznaczonego do zarządzania rezerwacjami obiektów sportowych skupionych w ramach jednego dużego kompleksu sportowego. Głównym celem aplikacji jest umożliwienie użytkownikom łatwego i szybkiego dokonywania rezerwacji na różne zajęcia sportowe (mecze w tenisa, padla, squasha, badmintona i piłkę nożną), przy jednoczesnym zapewnieniu administratorom placówki narzędzi do pełnej kontroli nad dostępnością i harmonogramem obiektów.

System został zaprojektowany z myślą o wieloplatformowości, działa zatem w środowisku kontenerowym Docker, co umożliwia łatwe wdrażanie i skalowanie API w różnych środowiskach produkcyjnych. Wykorzystanie bazy danych SQL Server zapewnia przechowywanie informacji o użytkownikach, rezerwacjach oraz szczegółach harmonogramu i dostępności poszczególnych obiektów sportowych.

---

### Wykorzystane technologie
API zostało zaimplementowane w technologii .NET 8 z wykorzystaniem MinimalAPI.

Warstwa persystencji opiera się o Entity Framework Córę 9 z bazą danych SQL Server uruchomioną w środowisku kontenerowym Docker.

Migracje bazy danych odbywają się automatycznie i są zarządzane za pomocą narzędzia EF Core Migrations.

System uwierzytelniania wykorzystuje mechanizm Basic Authentication z dwoma predefiniowanymi użytkownikami: “admin” z uprawnieniami administratora oraz “user” z uprawnieniami standardowego użytkownika (możliwość zmiany nazw, haseł oraz dodania innych użytkowników poprzez modyfikację pliku `SportsCenterBookingSystem/SportsCenter.API/Extensions/Auth/BasicAuthHandler.cs`).

Baza danych typu in-memory Redis jest wykorzystywana jako cache dla (...)

System posiada także zaimplementowany rate limiting dla pojedynczego użytkownika, chroniący API przed nadmiernym obciążeniem, co testowane jest przy wykorzystaniu narzędzia NBomber.

Dokumentacja API jest generowana automatycznie przy użyciu Swagger/OpenAPI, a kod źródłowy posiada dodatkowo przygotowany plik `SportsCenterBookingSystem/sportscenter_api.postman_collection.json` będący kolekcją Postmana do łatwiejszego wykonywania i testowania zapytań i odpowiedzi API.

---

### Wdrożenie systemu, populacja bazy danych przykładowymi encjami i testy zapytań