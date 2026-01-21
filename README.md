<p align="center">
  <img src="assets/logo.png" alt="SportsCenterBookingSystem logo" width=“600"/>
</p>

## Wprowadzenie i cel projektu
Celem projektu było stworzenie kompleksowego systemu API przeznaczonego do zarządzania rezerwacjami obiektów sportowych skupionych w ramach jednego dużego kompleksu sportowego. Głównym celem aplikacji jest umożliwienie użytkownikom łatwego i szybkiego dokonywania rezerwacji na różne zajęcia sportowe (mecze w tenisa, padla, squasha, badmintona i piłkę nożną), przy jednoczesnym zapewnieniu administratorom placówki narzędzi do pełnej kontroli nad dostępnością i harmonogramem obiektów.

System został zaprojektowany z myślą o wieloplatformowości, działa zatem w środowisku kontenerowym Docker, co umożliwia łatwe wdrażanie i skalowanie API w różnych środowiskach produkcyjnych. Wykorzystanie bazy danych SQL Server zapewnia przechowywanie informacji o użytkownikach, rezerwacjach oraz szczegółach harmonogramu i dostępności poszczególnych obiektów sportowych.


## Wykorzystane technologie
- API zostało zaimplementowane w technologii .NET 8 z wykorzystaniem MinimalAPI.

- Warstwa persystencji opiera się o Entity Framework Córę 9 z bazą danych SQL Server uruchomioną w środowisku kontenerowym Docker.

- Migracje bazy danych odbywają się automatycznie i są zarządzane za pomocą narzędzia EF Core Migrations.

- System uwierzytelniania wykorzystuje mechanizm Basic Authentication z dwoma predefiniowanymi użytkownikami: “admin” z uprawnieniami administratora oraz “user” z uprawnieniami standardowego użytkownika (możliwość zmiany nazw, haseł oraz dodania innych użytkowników poprzez modyfikację pliku `SportsCenterBookingSystem/SportsCenter.API/Extensions/Auth/BasicAuthHandler.cs`).

- Baza danych typu in-memory Redis jest wykorzystywana do zapisania w pamięci podręcznej query mówiącego o tym, jakie obiekty sportowe są stworzone w ramach całego kompleksu. Cache jest odświeżany co 24 godziny.

- System posiada także zaimplementowany rate limiting dla pojedynczego użytkownika, chroniący API przed nadmiernym obciążeniem, co testowane jest przy wykorzystaniu narzędzia NBomber.

- Dokumentacja API jest generowana automatycznie przy użyciu Swagger/OpenAPI, a kod źródłowy posiada dodatkowo przygotowany plik `SportsCenterBookingSystem/sportscenter_api.postman_collection.json` będący kolekcją Postmana do łatwiejszego wykonywania i testowania zapytań i odpowiedzi API.


## Wdrożenie systemu, populacja bazy danych przykładowymi encjami i testy zapytań

### Wdrożenie i pierwsze uruchomienie aplikacji
Aby wdrożyć i uruchomić aplikację, konieczne jest uprzednie zainstalowanie środowiska kontenerowego Docker i Docker Compose (oba są zawarte w np. Docker Desktop). Następnie dla środowiska Linux/x64 lub macOS należy kolejno:

1. Pobrać z repozytorium plik `docker-compose-prod.yml` lub skopiować jego zawartość do własnego pliku. Przy okazji tego kroku można także zmienić domyślne hasło do bazy danych SQL Server,

2. Z poziomu katalogu, w którym znajduje się `docker-compose-prod.yml`, należy uruchomić aplikację komendą:
    ```bash
    docker-compose -f docker-compose.prod.yml up -d
    ```

	Po uruchomieniu aplikacji w Docker Desktop lub po sprawdzeniu statusu kontenerów przy użyciu komendy
	```
	docker-compose -f docker-compose.prod.yml ps
	```
    powinny pojawić się 3 działające kontenery:
    - `sportscenter_api` – API aplikacji,
    - `sportscenter_sqlserver` – baza danych SQL Server dla aplikacji,
    - `sportscenter_redis` – cache Redis dla obiektów sportowych.


3. Otworzyć w przeglądarce nową kartę pod adresem `http://localhost:5001/swagger/`, co zapewni dostęp do dokumentacji Swagger/OpenAPI. W tym momencie można już dowolnie testować REST API.

### Populacja bazy danych przykładowymi encjami
Aby spopulować bazę danych aplikacji przykładowymi wartościami, należy kolejno:

1. Pobrać pliki `seed-data.sql` oraz `seed.sh` z repozytorium,

2. Uruchomić skrypt populujący przy użyciu komendy:
    ```
  	sh seed.sh
    ```

Jeżeli populacja bazy danych się powiedzie, skrypt poinformuje o sukcesie wykonania.

### Testy zapytań
Na tym poziomie aplikacja jest gotowa do testowania. Testy zapytań można wykonywać z poziomu SwaggerUI lub przy wykorzystaniu dowolnego narzędzia do testowania enepointów, np. Postman. Kolekcja Postmana przygotowana konkretnie dla tej aplikacji jest dostępna w repozytorium, a jej opis znajduje się niżej w readme.