# Task Manager - Local Development Guide

## Quick Start URLs

- **Frontend (React)**: http://localhost:5173/
- **Backend API (.NET)**: http://localhost:5214
- **Swagger API Docs**: http://localhost:5214/swagger
- **Adminer (Database UI)**: http://localhost:8080
- **PostgreSQL Database**: localhost:5432

---

## Database Access

### PostgreSQL Connection Information
```
Host: localhost
Port: 5432
Database: TaskManagerDb
Username: taskmanageradmin
Password: TaskManager123!
SSL Mode: Disable
```

### Full Connection String
```
Host=localhost;Port=5432;Database=TaskManagerDb;Username=taskmanageradmin;Password=TaskManager123!;SSL Mode=Disable
```

### Adminer Web UI Login
1. Navigate to: http://localhost:8080
2. Enter credentials:
   - **System**: PostgreSQL
   - **Server**: postgres
   - **Username**: taskmanageradmin
   - **Password**: TaskManager123!
   - **Database**: TaskManagerDb

---

## Starting the Application

### 1. Start Docker Services (Database)
```bash
cd /Users/carlos.reyesmunoz/Documents/projects/taskManager
docker-compose up -d
```

This starts:
- PostgreSQL 15 container (taskmanager-postgres)
- Adminer container (taskmanager-adminer)

### 2. Verify Docker Containers
```bash
docker ps
```

Should show both containers running.

### 3. Apply Database Migrations (First Time Only)
```bash
cd backend/TaskManager.Api
dotnet ef database update
```

### 4. Start Backend API
```bash
cd backend/TaskManager.Api
dotnet run
```

Backend will start on: http://localhost:5214

### 5. Start Frontend
```bash
cd frontend
npm run dev
```

Frontend will start on: http://localhost:5173

---

## Stopping the Application

### Stop Frontend
Press `Ctrl+C` in the terminal running `npm run dev`

### Stop Backend
Press `Ctrl+C` in the terminal running `dotnet run`

### Stop Docker Services
```bash
docker-compose down
```

### Stop Docker and Remove Data
```bash
docker-compose down -v
```
⚠️ This will delete all database data!

---

## Database Schema

### Tables Created
1. **Organizations**
   - Id, Name, Description, OwnerId, CreatedAt, UpdatedAt
   
2. **Users**
   - Id, Name, Email, Role (admin/user), OrganizationId, Points, IsActive, CreatedAt, UpdatedAt
   
3. **Tasks**
   - Id, Title, Description, Status (uncompleted/completed/finalized), DueDate
   - OrganizationId, CreatedById, AssignedToId, PickedById, CompletedById, FinalizedById
   - CreatedAt, UpdatedAt, CompletedAt, PickedAt, FinalizedAt
   
4. **TaskHistories**
   - Id, TaskId, UserId, Action, OldStatus, NewStatus, Comments, Timestamp, OrganizationId
   
5. **UserInvitations**
   - Id, Email, Role, OrganizationId, InvitedById, Status, Token, ExpiresAt, SentAt

---

## API Endpoints

### Organizations
- `GET /api/organizations` - List all organizations
- `GET /api/organizations/{id}` - Get organization by ID
- `POST /api/organizations` - Create organization
- `PUT /api/organizations/{id}` - Update organization
- `DELETE /api/organizations/{id}` - Delete organization
- `GET /api/organizations/{id}/users` - Get organization users
- `GET /api/organizations/{id}/tasks` - Get organization tasks

### Users
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `GET /api/users/{id}/tasks` - Get user's tasks
- `GET /api/users/{id}/assigned-tasks` - Get tasks assigned to user
- `GET /api/users/{id}/history` - Get user's task history

### Tasks
- `GET /api/tasks` - List all tasks
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/tasks/{id}/assign` - Assign task to user
- `POST /api/tasks/{id}/pick` - Pick task from pool
- `POST /api/tasks/{id}/complete` - Mark task as completed
- `POST /api/tasks/{id}/finalize` - Finalize task (awards points)
- `GET /api/tasks/{id}/history` - Get task history
- `GET /api/tasks/organization/{organizationId}/available` - Get available tasks

### User Invitations
- `GET /api/userinvitations` - List all invitations
- `GET /api/userinvitations/{id}` - Get invitation by ID
- `POST /api/userinvitations` - Create invitation
- `PUT /api/userinvitations/{id}` - Update invitation
- `DELETE /api/userinvitations/{id}` - Delete invitation
- `POST /api/userinvitations/{id}/accept` - Accept invitation
- `GET /api/userinvitations/organization/{organizationId}` - Get organization invitations

---

## Environment Variables

### Backend (.NET)
Location: `backend/TaskManager.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskManagerDb;Username=taskmanageradmin;Password=TaskManager123!;SSL Mode=Disable"
  },
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://localhost:5173"
  ]
}
```

### Frontend (React)
Location: `frontend/.env.local`

```
VITE_API_URL=http://localhost:5214/api
VITE_ENVIRONMENT=development
```

---

## Troubleshooting

### PostgreSQL Connection Issues

**Problem**: "role 'taskmanageradmin' does not exist"

**Solution**: Stop local PostgreSQL if running
```bash
brew services stop postgresql@17
```

### Port Already in Use

**Backend (5214)**:
```bash
lsof -ti:5214 | xargs kill -9
```

**Frontend (5173)**:
```bash
lsof -ti:5173 | xargs kill -9
```

**PostgreSQL (5432)**:
```bash
lsof -ti:5432 | xargs kill -9
```

### Docker Issues

**Reset Docker Containers**:
```bash
docker-compose down -v
docker-compose up -d
cd backend/TaskManager.Api
dotnet ef database update
```

### Database Migration Issues

**Reset Migrations**:
```bash
cd backend/TaskManager.Api
dotnet ef database drop
dotnet ef database update
```

---

## Azure Deployment Information

### Resource Groups
- **Development**: taskmanager-dev
- **Production**: taskmanager-prod

### Azure Resources (When Deployed)
- **PostgreSQL Flexible Server**: B1ms tier (Free tier eligible)
- **App Service**: F1 tier (Free)
- **Static Web App**: Free tier
- **Key Vault**: Standard tier
- **Application Insights**: Pay-as-you-go
- **Log Analytics Workspace**: Pay-as-you-go

### GitHub Actions Workflows
1. `deploy-infrastructure.yml` - Deploy Bicep templates
2. `deploy-backend.yml` - Deploy .NET API to App Service
3. `deploy-frontend.yml` - Deploy React app to Static Web App

---

## Project Structure

```
taskManager/
├── infrastructure/           # Azure Bicep templates
│   ├── main.bicep
│   ├── modules/
│   │   ├── keyvault.bicep
│   │   ├── database.bicep
│   │   ├── appservice.bicep
│   │   └── staticwebapp.bicep
│   └── parameters/
├── backend/
│   └── TaskManager.Api/
│       ├── Controllers/      # API endpoints
│       ├── Models/          # Entity models
│       ├── Services/        # Business logic
│       ├── Data/            # EF Core context
│       └── Program.cs       # Startup configuration
├── frontend/
│   └── src/
│       ├── components/      # React components
│       ├── services/        # API services
│       ├── types/          # TypeScript types
│       └── App.tsx
├── docker-compose.yml       # PostgreSQL + Adminer
└── .github/workflows/       # CI/CD pipelines
```

---

## Development Workflow

### Daily Development
1. Start Docker: `docker-compose up -d`
2. Start Backend: `cd backend/TaskManager.Api && dotnet run`
3. Start Frontend: `cd frontend && npm run dev`
4. Code and test
5. Stop services when done

### Database Changes
1. Modify models in `backend/TaskManager.Api/Models/`
2. Create migration: `dotnet ef migrations add MigrationName`
3. Apply migration: `dotnet ef database update`

### API Testing
- Use Swagger UI: http://localhost:5214/swagger
- Or use tools like Postman, curl, or HTTPie

---

## Important Notes

- **Local PostgreSQL**: Make sure local PostgreSQL service is stopped to avoid conflicts with Docker
- **CORS**: Configured for localhost:5173 and Azure Static Web Apps
- **Entity Framework**: Auto-creates database in development, uses migrations in production
- **Points System**: Users earn points when tasks are finalized
- **Multi-tenant**: All data is scoped by OrganizationId
- **Task Workflow**: uncompleted → assigned/picked → completed → finalized

---

## Contact & Documentation

- **README**: See `README.md` for general project information
- **Deployment Guide**: See `DEPLOYMENT.md` for Azure deployment instructions
- **Database Diagram**: See `taskManager.dbdiagram` for visual schema
- **App Flow**: See `appFlow.md` for application workflow

---

*Last Updated: December 18, 2024*
