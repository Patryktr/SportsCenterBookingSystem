<div align="center">
  <p>System rezerwacji obiektów sportowych</p>
  <img src="assets/logo.png" width=“600"/>
</div>



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


## Wdrożenie systemu, populacja bazy danych przykładowymi encjami i testy zapytań (Swagger/OpenAPI, Postman)

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
Na tym poziomie aplikacja jest gotowa do testowania. Testy zapytań można wykonywać z poziomu SwaggerUI lub przy wykorzystaniu dowolnego narzędzia do testowania enepointów, np. Postman.

Projekt posiada przygotowaną kolekcję Postmana do testowania zapytań wraz z kilkoma scenariuszami testowymi, uwzględniającymi między innymi następujące sytuacje:
- pełny cykl rezerwacyjny polegający kolejno na:
  - wyszukaniu, jakimi obiektami sportowymi dysponuje placówka,
  - wyszukaniu wolnych slotów na wybranym obiekcie w dniu, który interesuje użytkownika tworzącego rezerwację,
  - podwójnej weryfikacji dostępności obiektu poprzez jej sprawdzenie w konkretnych godzinach,
  - utworzeniu rezerwacji na wybranym obiekcie w wybranym przedziale czasowym,
  - upewnieniu się, że rezerwacja została utworzona pomyślnie, poprzez weryfikację dostępności obiektu w godzinach, w których została ona utworzona,
- walidacja kilku przykładowych błędów:
  - próba utworzenia rezerwacji w blokach czasowych niedozwolonych przez administratora zarządzającego placówką, tj. nie o pełnej godzinie lub nie na czas będący iloczynem jednej godziny,
  - próba utworzenia rezerwacji na konkretny obiekt dla `x` graczy, gdzie `x` jest większe niż ilość graczy dopuszczalna do jednoczesnego użytkowania obiektu sportowego,
  - próba pobrania informacji o obiekcie sportowym, który nie istnieje,
  - próba pobrania informacji o rezerwacji utworzonej przez klienta o konkretnym GUID, który nie istnieje.

Aby zaimportować kolekcję do Postmana, należy z paska menu aplikacji wybrać opcję `Import`, a następnie wskazać plik z rozszerzeniem `.json` będący kolekcją. Przykładowa kolekcja, o której mowa w akapicie wyżej, znajduje się w repozytorium w katalogu głównym projektu pod nazwą `sportscenter_api.postman_collection.json`.

## Testy aplikacji
Kod źródłowy aplikacji posiada także wbudowane testy warstwy biznesowej i wydajności API.

### Testy jednostkowe

##### 1. Testy kolizji rezerwacji:
1. `BookingsOverlap_ShouldDetectCollisionsCorrectly`,
2. `CancelledBooking_ShouldNotCauseCollision`,
3. `DifferentFacilities_ShouldNotCauseCollision`,
4. `MultipleBookings_ShouldDetectAnyCollision`,
5. `MultipleBookings_ShouldDetectAnyCollision`.

##### 2. Testy walidacyjne encji obiektów sportowych i ich ograniczeń:
1. `Facility_DefaultValues_ShouldBeCorrect`,
2. `Facility_DurationLimits_ShouldBeValid`,
3. `Facility_SportType_ShouldHaveReasonableMaxPlayers`,
4. `Facility_MaxPlayers_ShouldNotBeZeroOrNegative`,
5. `Facility_PricePerHour_ShouldBePositive`,
6. `Facility_WhenInactive_ShouldNotAllowBookings`,
7. `Facility_Name_ShouldNotBeEmpty`.

##### 3. Testy walidacyjne wymagań rezerwacji na pełne godziny:
1. `IsFullHour_ShouldValidateCorrectly`,
2. `BookingTimes_WhenBothFullHours_ShouldBeValid`,
3. `BookingTimes_WhenStartNotFullHour_ShouldBeInvalid`,
4. `BookingTimes_WhenEndNotFullHour_ShouldBeInvalid`.

##### 4. Testy walidacyjne slotów godzinowych:
1. `TimeSlotItem_ShouldHaveCorrectProperties`,
2. `TimeSlotItem_Duration_ShouldBeOneHour`,
3. `TimeSlotStatus_ShouldHaveCorrectPolishName`,
4. `TimeSlotStatus_AllValuesShouldBeUnique`,
5. `GenerateHourlySlots_ShouldCreateCorrectSlots`.

### Testy integracyjne

##### 1. Testy logiki anulowania rezerwacji z idempotencją:
1. `Handle_WithValidBookingMoreThanOneHourAhead_ShouldCancel`,
2. `Handle_WithBookingLessThanOneHourAhead_ShouldReturnTooLate`,
3. `Handle_WithNonExistentBooking_ShouldReturnNotFound`,
4. `Handle_WithAlreadyCancelledBooking_ShouldReturnAlreadyCancelled_Idempotent`,
5. `Handle_CalledTwice_ShouldBeIdempotent`,
6. `Handle_WithExactlyOneHourBefore_ShouldCancel`.

##### 2. Testy sprawdzania dostępności terminu:
1. `Handle_WhenSlotAvailable_ShouldReturnIsAvailableTrue`,
2. `Handle_WhenSlotBooked_ShouldReturnIsAvailableFalse`,
3. `Handle_WhenFacilityNotExists_ShouldReturnIsAvailableFalse`,
4. `Handle_WhenFacilityInactive_ShouldReturnIsAvailableFalse`,
5. `Handle_WhenCanceledBookingExists_ShouldReturnIsAvailableTrue`,
6. `Handle_WhenBookingDoesNotOverlap_ShouldReturnIsAvailableTrue`.

##### 3. Testy tworzenia rezerwacji:
1. `Handle_WithValidRequest_ShouldCreateBooking`,
2. `Handle_WithConflictingBooking_ShouldReturnFailure`,
3. `Handle_WithStartNotFullHour_ShouldReturnFailure`,
4. `Handle_WithEndNotFullHour_ShouldReturnFailure`,
5. `Handle_WithFullHours_ShouldSucceed`,
6. `Handle_WithTooManyPlayers_ShouldReturnFailure`,
7. `Handle_WithInactiveFacility_ShouldReturnFailure`,
8. `Handle_WithStartInPast_ShouldReturnFailure`,
9. `Handle_WithNonExistentCustomer_ShouldReturnFailure`,
10. `Handle_WithNonExistentFacility_ShouldReturnFailure`,
11. `Handle_WithZeroPlayers_ShouldReturnFailure`,
12. `Handle_WithStartAfterEnd_ShouldReturnFailure`.

##### 4. Testy wyszukiwania dostępnych slotów:
1. `Handle_WithValidRequest_ShouldReturnSlots`,
2. `Handle_WithNonExistentFacility_ShouldReturnFailure`,
3. `Handle_WithInactiveFacility_ShouldReturnFailure`,
4. `Handle_WithExistingBooking_ShouldMarkSlotAsBooked`,
5. `Handle_ShouldReturnCorrectMessage_WhenAllAvailable`,
6. `Handle_Slots_ShouldHaveCorrectTimeFormat`,
7. `Handle_Slots_ShouldBeInChronologicalOrder`,
8. `Handle_EachSlot_ShouldBeExactlyOneHour`.

### Testy wydajnościowe API:

##### 1. Load Test endpointów GET aplikacji:
1. `GetFacilities_LoadTest`,
2. `GetFacilityById_LoadTest`,
3. `GetBookings_LoadTest`,
4. `GetBookingById_LoadTest`,
5. `GetCustomers_LoadTest`.

##### 2. Load Test endpointów związanych ze sprawdzaniem i wyszukiwaniem dostępności:
1. `CheckAvailability_LoadTest`,
2. `SearchAvailability_LoadTest`.

##### 3. Load Test kilku endpointów CREATE aplikacji:
1. `CreateCustomer_LoadTest`,
2. `CreateFacility_LoadTest`,
3. `CreateBooking_LoadTest`.

##### 4. Load Test kilku endpointów UPDATE i DELETE aplikacji:
1. `UpdateFacility_LoadTest`,
2. `UpdateBooking_LoadTest`,
3. `DeleteBooking_LoadTest`.

##### 5. Stress Test aplikacji:
1. `MixedReadOperations_StressTest`,
2. `MixedReadWriteOperations_StressTest`,
3. `HighConcurrency_StressTest`.

##### 6. Spike Test aplikacji:
1. `PeakLoad_SpikeTest`,
2. `DoubleSpike_StressTest`.

##### 7. Endurance Test aplikacji:
1. `Endurance_LongRunningTest`,
2. `Endurance_MixedOperations_LongRunningTest`.