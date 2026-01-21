USE SportsCenterDb;
GO

-- Wyczyść istniejące dane (opcjonalne - odkomentuj jeśli potrzebujesz)
-- DELETE FROM Bookings;
-- DELETE FROM TimeBlocks;
-- DELETE FROM OperatingHours;
-- DELETE FROM Customers;
-- DELETE FROM Facilities;

-- =============================================================================
-- 1. CUSTOMERS (Klienci)
-- =============================================================================
PRINT 'Dodawanie klientów...';

-- Sprawdź czy klienci już istnieją
IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'jan.kowalski@email.pl')
BEGIN
    INSERT INTO Customers (PublicId, FirstName, LastName, Email, Phone) VALUES
    (NEWID(), 'Jan', 'Kowalski', 'jan.kowalski@email.pl', '+48 600 100 001'),
    (NEWID(), 'Anna', 'Nowak', 'anna.nowak@email.pl', '+48 600 100 002'),
    (NEWID(), 'Piotr', 'Wiśniewski', 'piotr.wisniewski@email.pl', '+48 600 100 003'),
    (NEWID(), 'Maria', 'Wójcik', 'maria.wojcik@email.pl', '+48 600 100 004'),
    (NEWID(), 'Tomasz', 'Kamiński', 'tomasz.kaminski@email.pl', '+48 600 100 005'),
    (NEWID(), 'Katarzyna', 'Lewandowska', 'katarzyna.lewandowska@email.pl', NULL),
    (NEWID(), 'Michał', 'Zieliński', 'michal.zielinski@email.pl', '+48 600 100 007'),
    (NEWID(), 'Agnieszka', 'Szymańska', 'agnieszka.szymanska@email.pl', '+48 600 100 008');
    
    PRINT 'Dodano 8 klientów.';
END
ELSE
BEGIN
    PRINT 'Klienci już istnieją - pomijam.';
END

-- =============================================================================
-- 2. FACILITIES (Obiekty sportowe)
-- =============================================================================
PRINT 'Dodawanie obiektów sportowych...';

-- SportType: Tennis=1, Football=2, Padel=3, Squash=4, Badminton=5
IF NOT EXISTS (SELECT 1 FROM Facilities WHERE Name = 'Kort Tenisowy #1')
BEGIN
    INSERT INTO Facilities (Name, SportType, MaxPlayers, PricePerHour, IsActive, MinBookingDurationMinutes, MaxBookingDurationMinutes) VALUES
    -- Korty tenisowe
    ('Kort Tenisowy #1', 1, 4, 80.00, 1, 60, 180),
    ('Kort Tenisowy #2', 1, 4, 80.00, 1, 60, 180),
    ('Kort Tenisowy #3 (kryty)', 1, 4, 120.00, 1, 60, 240),
    
    -- Boiska piłkarskie
    ('Boisko Piłkarskie - Orlik', 2, 22, 150.00, 1, 60, 120),
    ('Boisko Piłkarskie - Hala', 2, 14, 200.00, 1, 60, 120),
    
    -- Korty do padla
    ('Kort Padel #1', 3, 4, 100.00, 1, 60, 120),
    ('Kort Padel #2', 3, 4, 100.00, 1, 60, 120),
    
    -- Korty do squasha
    ('Kort Squash #1', 4, 4, 60.00, 1, 30, 120),
    ('Kort Squash #2', 4, 4, 60.00, 1, 30, 120),
    
    -- Korty do badmintona
    ('Kort Badminton #1', 5, 4, 50.00, 1, 60, 120),
    ('Kort Badminton #2', 5, 4, 50.00, 0, 60, 120); -- Nieaktywny (w remoncie)
    
    PRINT 'Dodano 11 obiektów sportowych.';
END
ELSE
BEGIN
    PRINT 'Obiekty sportowe już istnieją - pomijam.';
END

-- =============================================================================
-- 3. OPERATING HOURS (Godziny otwarcia)
-- =============================================================================
PRINT 'Dodawanie godzin otwarcia...';

-- DayOfWeek: Sunday=0, Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5, Saturday=6
DECLARE @FacilityId INT;

-- Dla każdego aktywnego obiektu ustaw godziny otwarcia
DECLARE facility_cursor CURSOR FOR 
    SELECT Id FROM Facilities WHERE IsActive = 1;

OPEN facility_cursor;
FETCH NEXT FROM facility_cursor INTO @FacilityId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Sprawdź czy godziny już istnieją dla tego obiektu
    IF NOT EXISTS (SELECT 1 FROM OperatingHours WHERE FacilityId = @FacilityId)
    BEGIN
        -- Poniedziałek - Piątek: 07:00 - 22:00
        INSERT INTO OperatingHours (FacilityId, DayOfWeek, OpenTime, CloseTime, IsClosed) VALUES
        (@FacilityId, 1, '07:00:00', '22:00:00', 0), -- Poniedziałek
        (@FacilityId, 2, '07:00:00', '22:00:00', 0), -- Wtorek
        (@FacilityId, 3, '07:00:00', '22:00:00', 0), -- Środa
        (@FacilityId, 4, '07:00:00', '22:00:00', 0), -- Czwartek
        (@FacilityId, 5, '07:00:00', '22:00:00', 0), -- Piątek
        -- Sobota: 08:00 - 20:00
        (@FacilityId, 6, '08:00:00', '20:00:00', 0), -- Sobota
        -- Niedziela: 09:00 - 18:00
        (@FacilityId, 0, '09:00:00', '18:00:00', 0); -- Niedziela
    END
    
    FETCH NEXT FROM facility_cursor INTO @FacilityId;
END

CLOSE facility_cursor;
DEALLOCATE facility_cursor;

PRINT 'Dodano godziny otwarcia dla wszystkich aktywnych obiektów.';

-- =============================================================================
-- 4. TIME BLOCKS (Blokady terminów)
-- =============================================================================
PRINT 'Dodawanie blokad terminów...';

-- BlockType: Maintenance=1, SpecialEvent=2, Holiday=3, Other=4
DECLARE @Tomorrow DATE = DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE));
DECLARE @NextWeek DATE = DATEADD(DAY, 7, CAST(GETUTCDATE() AS DATE));

IF NOT EXISTS (SELECT 1 FROM TimeBlocks WHERE Reason = 'Konserwacja nawierzchni')
BEGIN
    INSERT INTO TimeBlocks (FacilityId, BlockType, StartTime, EndTime, Reason, IsActive, CreatedAt) VALUES
    -- Przerwa techniczna na korcie tenisowym
    (1, 1, DATEADD(HOUR, 8, CAST(@Tomorrow AS DATETIME)), DATEADD(HOUR, 12, CAST(@Tomorrow AS DATETIME)), 
        'Konserwacja nawierzchni', 1, GETUTCDATE()),
    
    -- Wydarzenie specjalne - turniej na boisku
    (4, 2, DATEADD(HOUR, 10, CAST(@NextWeek AS DATETIME)), DATEADD(HOUR, 18, CAST(@NextWeek AS DATETIME)), 
        'Turniej Młodzieżowy - Puchar Prezydenta Miasta', 1, GETUTCDATE()),
    
    -- Święto - zamknięcie
    (1, 3, DATEADD(DAY, 14, CAST(GETUTCDATE() AS DATETIME)), DATEADD(DAY, 15, CAST(GETUTCDATE() AS DATETIME)), 
        'Święto państwowe', 1, GETUTCDATE());
    
    PRINT 'Dodano 3 blokady terminów.';
END
ELSE
BEGIN
    PRINT 'Blokady terminów już istnieją - pomijam.';
END

-- =============================================================================
-- 5. BOOKINGS (Rezerwacje)
-- =============================================================================
PRINT 'Dodawanie rezerwacji...';

-- BookingStatus: Active=1, Canceled=2
-- BookingType: Exclusive=1, GroupClass=2

DECLARE @CustomerId1 INT, @CustomerId2 INT, @CustomerId3 INT, @CustomerId4 INT;
DECLARE @Today DATE = CAST(GETUTCDATE() AS DATE);

-- Pobierz ID klientów
SELECT TOP 1 @CustomerId1 = Id FROM Customers WHERE Email = 'jan.kowalski@email.pl';
SELECT TOP 1 @CustomerId2 = Id FROM Customers WHERE Email = 'anna.nowak@email.pl';
SELECT TOP 1 @CustomerId3 = Id FROM Customers WHERE Email = 'piotr.wisniewski@email.pl';
SELECT TOP 1 @CustomerId4 = Id FROM Customers WHERE Email = 'maria.wojcik@email.pl';

IF @CustomerId1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Bookings WHERE CustomerId = @CustomerId1)
BEGIN
    -- Rezerwacje na jutro
    INSERT INTO Bookings (FacilityId, CustomerId, Start, [End], PlayersCount, TotalPrice, Status, Type) VALUES
    -- Aktywne rezerwacje na jutro
    (1, @CustomerId1, 
        DATEADD(HOUR, 14, CAST(@Tomorrow AS DATETIME)), 
        DATEADD(HOUR, 16, CAST(@Tomorrow AS DATETIME)), 
        2, 160.00, 1, 1),
    
    (3, @CustomerId2, 
        DATEADD(HOUR, 10, CAST(@Tomorrow AS DATETIME)), 
        DATEADD(HOUR, 12, CAST(@Tomorrow AS DATETIME)), 
        4, 240.00, 1, 1),
    
    (6, @CustomerId3, 
        DATEADD(HOUR, 18, CAST(@Tomorrow AS DATETIME)), 
        DATEADD(HOUR, 19, CAST(@Tomorrow AS DATETIME)), 
        4, 100.00, 1, 1),
    
    -- Rezerwacje na następny tydzień
    (4, @CustomerId4, 
        DATEADD(HOUR, 16, CAST(@NextWeek AS DATETIME)), 
        DATEADD(HOUR, 18, CAST(@NextWeek AS DATETIME)), 
        14, 300.00, 1, 2), -- Zajęcia grupowe
    
    (8, @CustomerId1, 
        DATEADD(HOUR, 19, CAST(@NextWeek AS DATETIME)), 
        DATEADD(HOUR, 20, CAST(@NextWeek AS DATETIME)), 
        2, 60.00, 1, 1),
    
    -- Anulowana rezerwacja (dla demonstracji)
    (2, @CustomerId2, 
        DATEADD(HOUR, 9, CAST(@Tomorrow AS DATETIME)), 
        DATEADD(HOUR, 10, CAST(@Tomorrow AS DATETIME)), 
        2, 80.00, 2, 1),
    
    -- Rezerwacje w przyszłości (za 2 tygodnie)
    (1, @CustomerId3, 
        DATEADD(HOUR, 10, DATEADD(DAY, 14, CAST(@Today AS DATETIME))), 
        DATEADD(HOUR, 12, DATEADD(DAY, 14, CAST(@Today AS DATETIME))), 
        4, 160.00, 1, 1),
    
    (7, @CustomerId4, 
        DATEADD(HOUR, 15, DATEADD(DAY, 14, CAST(@Today AS DATETIME))), 
        DATEADD(HOUR, 17, DATEADD(DAY, 14, CAST(@Today AS DATETIME))), 
        4, 200.00, 1, 1);
    
    PRINT 'Dodano 8 rezerwacji.';
END
ELSE
BEGIN
    PRINT 'Rezerwacje już istnieją lub brak klientów - pomijam.';
END

-- =============================================================================
-- PODSUMOWANIE
-- =============================================================================
PRINT '';
PRINT '=== PODSUMOWANIE SEEDOWANIA ===';
PRINT '';

SELECT 'Customers' AS Tabela, COUNT(*) AS Liczba FROM Customers
UNION ALL
SELECT 'Facilities', COUNT(*) FROM Facilities
UNION ALL
SELECT 'OperatingHours', COUNT(*) FROM OperatingHours
UNION ALL
SELECT 'TimeBlocks', COUNT(*) FROM TimeBlocks
UNION ALL
SELECT 'Bookings', COUNT(*) FROM Bookings;

PRINT '';
PRINT 'Seedowanie zakończone pomyślnie!';
PRINT '';
PRINT 'Dane dostępowe do API:';
PRINT '  - User:  user / 123';
PRINT '  - Admin: admin / 123';
PRINT '';
GO