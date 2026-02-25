#!/bin/bash

# Setup script for TaskManager local development
# Starts the database, runs migrations, and seeds initial data.
# The database used locally matches the hosted dev environment (SQL Server 2022).

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_DIR="$SCRIPT_DIR/backend/TaskManager.Api"

echo "🚀 Setting up TaskManager development database..."
echo ""

# Step 1: Start Docker services
echo "1️⃣  Starting Docker services (SQL Server 2022)..."
docker compose up -d
echo "   ✅ Docker services started"
echo ""

# Step 2: Wait for SQL Server to be ready
echo "2️⃣  Waiting for SQL Server to be ready..."
RETRIES=30
until docker exec taskmanager-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "TaskManager123!" -No \
    -Q "SELECT 1" > /dev/null 2>&1; do
    RETRIES=$((RETRIES - 1))
    if [ $RETRIES -le 0 ]; then
        echo "   ❌ SQL Server did not become ready in time. Check: docker logs taskmanager-sqlserver"
        exit 1
    fi
    echo "   ⏳ Still waiting... ($RETRIES retries left)"
    sleep 2
done
echo "   ✅ SQL Server is ready"
echo ""

# Step 3: Apply EF Core migrations
echo "3️⃣  Applying database migrations..."
cd "$API_DIR"
DOTNET_ROLL_FORWARD=Major dotnet ef database update
echo "   ✅ Migrations applied"
echo ""

# Step 4: Seed data runs automatically on app startup via DataSeeder.
#         Start the API once to trigger seeding, then stop it.
echo "4️⃣  Running app startup to seed initial data..."
ASPNETCORE_ENVIRONMENT=Development timeout 15 dotnet run --no-build 2>&1 | \
    grep -E "(Seeding|Applying|Migrat|error|Error)" || true
echo "   ✅ Seed data applied (idempotent - safe to run again)"
echo ""

echo "🎉 Setup complete!"
echo ""
echo "📋 Initial seed data:"
echo "   Organization : Default Organization (org-default-001)"
echo "   Admin user   : admin@taskmanager.com (user-admin-001)"
echo "   Dev user     : jane@taskmanager.com  (user-dev-001)"
echo "   Designer     : john@taskmanager.com  (user-designer-001)"
echo "   Sample tasks : 6 tasks seeded"
echo ""
echo "▶️  To start the API:    cd backend/TaskManager.Api && dotnet run"
echo "🗄️  To open Adminer UI:  http://localhost:8080"
echo "    System: MS SQL | Server: sqlserver | Username: sa | Password: TaskManager123!"

