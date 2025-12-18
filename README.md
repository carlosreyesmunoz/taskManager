# Task Manager

A multi-tenant task management application built with React (Vite), .NET 8 Web API, PostgreSQL, and deployed on Azure.

## Features

- **Multi-tenant organization system** - Isolated data per organization
- **Role-based access control** - Admin and User roles with different permissions
- **Task lifecycle management** - Create, assign, complete, and finalize tasks
- **Task pool system** - Shared pool where users can pick tasks
- **Points/gamification system** - Tasks award points to users upon completion
- **User invitation system** - Admins can invite users via email with token-based authentication
- **Task history tracking** - Complete audit trail of all task actions

## Tech Stack

### Frontend
- React 18 with TypeScript
- Vite for build tooling
- React Router for navigation
- Axios for API communication
- TanStack Query for data fetching

### Backend
- .NET 8 Web API
- Entity Framework Core 8
- PostgreSQL database
- Azure Key Vault for secrets management
- Azure Identity for authentication

### Infrastructure
- Azure App Service for backend hosting
- Azure Static Web Apps for frontend hosting
- Azure Database for PostgreSQL Flexible Server
- Azure Key Vault for secure configuration
- Azure Application Insights for monitoring
- Infrastructure as Code with Bicep

## Project Structure

```
taskManager/
├── frontend/                 # React Vite application
│   ├── src/
│   │   ├── components/      # Reusable React components
│   │   ├── pages/           # Page components
│   │   ├── services/        # API service layer
│   │   └── types/           # TypeScript type definitions
│   └── package.json
├── backend/                  # .NET Web API
│   └── TaskManager.Api/
│       ├── Controllers/     # API endpoints
│       ├── Models/          # Entity Framework models
│       ├── Data/            # DbContext and configurations
│       └── Services/        # Business logic layer
├── infrastructure/           # Bicep templates
│   ├── main.bicep           # Main infrastructure template
│   ├── modules/             # Modular Bicep files
│   ├── dev.bicepparam       # Development parameters
│   └── prod.bicepparam      # Production parameters
├── .github/
│   └── workflows/           # GitHub Actions CI/CD pipelines
├── docker-compose.yml       # Local development PostgreSQL
└── DEPLOYMENT.md            # Detailed deployment guide
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/get-started) (for local development)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd taskManager
   ```

2. **Start PostgreSQL database**
   ```bash
   docker-compose up -d
   ```

3. **Setup backend**
   ```bash
   cd backend/TaskManager.Api
   dotnet restore
   dotnet ef database update
   dotnet run
   ```
   
   API will be available at `https://localhost:5001`

4. **Setup frontend**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   
   Frontend will be available at `http://localhost:5173`

5. **Access database UI (Adminer)**
   - URL: `http://localhost:8080`
   - System: PostgreSQL
   - Server: postgres
   - Username: taskmanageradmin
   - Password: TaskManager123!
   - Database: TaskManagerDb

## Database Schema

The application uses a relational PostgreSQL database with the following main entities:

- **Organizations** - Multi-tenant containers
- **Users** - Role-based members (admin/user) with points
- **Tasks** - Work items with status workflow (uncompleted → completed → finalized)
- **Task History** - Complete audit trail
- **User Invitations** - Token-based email invitation system

See `taskManager.dbml` for complete schema details.

## Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for detailed deployment instructions.

### Quick Deploy to Azure

1. **Create Azure resources**
   ```bash
   az group create --name taskmanager-dev --location eastus
   az deployment group create \
     --resource-group taskmanager-dev \
     --template-file infrastructure/main.bicep \
     --parameters infrastructure/dev.bicepparam
   ```

2. **Configure GitHub Secrets**
   - `AZURE_CREDENTIALS`
   - `DB_ADMIN_USERNAME`
   - `DB_ADMIN_PASSWORD`
   - `AZURE_STATIC_WEB_APPS_API_TOKEN`

3. **Push to trigger deployment**
   ```bash
   git push origin main
   ```

## Environment Variables

### Backend (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TaskManagerDb;Username=taskmanageradmin;Password=TaskManager123!"
  },
  "KeyVaultName": "taskmanager-dev-kv-xxxxx"
}
```

### Frontend (`.env.local`)
```
VITE_API_URL=https://localhost:5001/api
VITE_ENVIRONMENT=development
```

## API Endpoints

### Organizations
- `GET /api/organizations` - List organizations
- `GET /api/organizations/{id}` - Get organization by ID
- `POST /api/organizations` - Create organization
- `PUT /api/organizations/{id}` - Update organization
- `DELETE /api/organizations/{id}` - Delete organization

### Users
- `GET /api/users/organization/{orgId}` - List users in organization
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user

### Tasks
- `GET /api/tasks/organization/{orgId}` - List tasks
- `GET /api/tasks/organization/{orgId}/pool` - Get task pool (unassigned tasks)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `POST /api/tasks/{id}/assign` - Assign task to user
- `POST /api/tasks/{id}/complete` - Mark task as completed
- `POST /api/tasks/{id}/finalize` - Finalize task (admin only)
- `DELETE /api/tasks/{id}` - Delete task

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please open an issue in the GitHub repository.
