#!/bin/bash
# =============================================================================
# SportsCenter - Skrypt seedujący bazę danych (Linux/macOS)
# =============================================================================

set -e

echo "=================================================="
echo "  SportsCenter - Seedowanie bazy danych"
echo "=================================================="
echo ""

# Konfiguracja
SQL_CONTAINER="sportscenter_sqlserver"
SA_PASSWORD="YourStrong@Password123"
SEED_FILE="seed-data.sql"

# Sprawdź czy plik SQL istnieje
if [ ! -f "$SEED_FILE" ]; then
    echo "Błąd: Plik $SEED_FILE nie istnieje!"
    echo "   Upewnij się, że uruchamiasz skrypt z katalogu zawierającego $SEED_FILE"
    exit 1
fi

# Sprawdź czy kontener SQL Server działa
if ! docker ps --format '{{.Names}}' | grep -q "^${SQL_CONTAINER}$"; then
    echo "Błąd: Kontener $SQL_CONTAINER nie jest uruchomiony!"
    echo "   Uruchom najpierw: docker-compose -f docker-compose.prod.yml up -d"
    exit 1
fi

echo "Kopiowanie skryptu SQL do kontenera..."
docker cp "$SEED_FILE" "${SQL_CONTAINER}:/tmp/seed-data.sql"

echo "Oczekiwanie na gotowość SQL Server (może potrwać do 60 sekund)..."
for i in {1..30}; do
    if docker exec "$SQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$SA_PASSWORD" \
        -Q "SELECT 1" -C -b > /dev/null 2>&1; then
        echo "SQL Server gotowy!"
        break
    fi
    echo "   Próba $i/30..."
    sleep 2
done

echo ""
echo "Uruchamianie skryptu seedującego..."
echo ""

docker exec "$SQL_CONTAINER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" \
    -d SportsCenterDb \
    -i /tmp/seed-data.sql \
    -C -b

echo ""
echo "=================================================="
echo "Seedowanie zakończone pomyślnie!"
echo "=================================================="
echo ""
echo "API dostępne pod adresem: http://localhost:5001"
echo "Swagger UI: http://localhost:5001/swagger"
echo ""
echo "Dane logowania:"
echo "  - User:  user / 123"
echo "  - Admin: admin / 123"
echo ""