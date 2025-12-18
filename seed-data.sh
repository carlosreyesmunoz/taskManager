#!/bin/bash

# Seed script for TaskManager application
# This creates initial test data to get started

API_URL="http://localhost:5214/api"

echo "🌱 Seeding TaskManager database..."
echo ""

# Step 1: Create initial user (without organization)
echo "1️⃣  Creating initial user directly in database..."
# We need to insert directly since we have a circular dependency

# For now, let's use SQL to insert the first user and organization
psql "postgresql://taskmanageradmin:TaskManager123!@localhost:5432/TaskManagerDb" << EOF
-- Temporarily disable foreign key constraints
SET session_replication_role = 'replica';

-- Insert initial user (with temp organization reference)
INSERT INTO "Users" ("Id", "Name", "Email", "Role", "OrganizationId", "Points", "IsActive", "CreatedAt", "UpdatedAt")
VALUES 
  ('admin-user-001', 'Admin User', 'admin@taskmanager.com', 'admin', 'temp-org-001', 0, true, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Insert initial organization
INSERT INTO "Organizations" ("Id", "Name", "Description", "OwnerId", "CreatedAt", "UpdatedAt")
VALUES 
  ('temp-org-001', 'Default Organization', 'Initial organization for testing', 'admin-user-001', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- Re-enable foreign key constraints
SET session_replication_role = 'origin';

-- Verify the data
SELECT 'User Created:' as status, "Id", "Name", "Email" FROM "Users" WHERE "Id" = 'admin-user-001';
SELECT 'Organization Created:' as status, "Id", "Name", "OwnerId" FROM "Organizations" WHERE "Id" = 'temp-org-001';
EOF

echo "✅ Initial admin user and organization created!"
echo ""
echo "📋 Initial Data:"
echo "   Organization ID: temp-org-001"
echo "   Organization Name: Default Organization"
echo "   User ID: admin-user-001"
echo "   User Email: admin@taskmanager.com"
echo ""
echo "🎉 You can now create additional users using the API!"
echo ""
echo "Example: Create a new user"
echo 'curl -X POST http://localhost:5214/api/users \'
echo '  -H "Content-Type: application/json" \'
echo '  -d '"'"'{'
echo '    "name": "John Doe",'
echo '    "email": "john@example.com",'
echo '    "role": "user",'
echo '    "organizationId": "temp-org-001",'
echo '    "isActive": true'
echo '  }'"'"
