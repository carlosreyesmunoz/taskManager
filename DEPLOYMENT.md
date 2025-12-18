# Task Manager - Deployment Guide

## Prerequisites

- Azure CLI installed and authenticated
- .NET 8 SDK
- Node.js 18+ (for frontend)
- Docker (for local development)

## Local Development Setup

### 1. Start PostgreSQL Database

```bash
docker-compose up -d
```

This will start:
- PostgreSQL on port 5432
- Adminer (database UI) on port 8080

### 2. Run Database Migrations

```bash
cd backend/TaskManager.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run Backend API

```bash
cd backend/TaskManager.Api
dotnet run
```

API will be available at: `https://localhost:5001`

### 4. Run Frontend (Vite)

```bash
cd frontend
npm install
npm run dev
```

Frontend will be available at: `http://localhost:5173`

## Azure Deployment

### 1. Create Resource Groups

```bash
# Development environment
az group create --name taskmanager-dev --location eastus

# Production environment
az group create --name taskmanager-prod --location eastus
```

### 2. Deploy Infrastructure with Bicep

```bash
# Deploy development environment
az deployment group create \
  --resource-group taskmanager-dev \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/dev.bicepparam

# Deploy production environment
az deployment group create \
  --resource-group taskmanager-prod \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/prod.bicepparam
```

### 3. Configure GitHub Secrets

Add the following secrets to your GitHub repository:

- `AZURE_CREDENTIALS` - Service principal credentials
- `DB_ADMIN_USERNAME` - PostgreSQL admin username
- `DB_ADMIN_PASSWORD` - PostgreSQL admin password

### 4. Deploy via GitHub Actions

Push to the main branch to trigger automatic deployment:

```bash
git add .
git commit -m "Initial deployment"
git push origin main
```

## Database Migrations on Azure

Run migrations against Azure database:

```bash
# Get connection string from Azure
az postgres flexible-server show-connection-string \
  --server-name <server-name> \
  --database-name TaskManagerDb

# Run migrations
cd backend/TaskManager.Api
dotnet ef database update --connection "<connection-string>"
```

## Environment Variables

### Backend (App Service)

- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `KeyVaultName` - Name of Azure Key Vault
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `APPLICATIONINSIGHTS_CONNECTION_STRING` - Application Insights connection

### Frontend (Static Web App)

- `REACT_APP_API_URL` - Backend API URL
- `REACT_APP_ENVIRONMENT` - dev/prod

## Monitoring

- Application Insights: `https://portal.azure.com` → Search for `taskmanager-{env}-ai`
- Log Analytics: View logs and queries
- Resource Health: Monitor resource availability

## Troubleshooting

### Backend not connecting to database

1. Check firewall rules in PostgreSQL Flexible Server
2. Verify connection string in Key Vault
3. Check App Service logs in Application Insights

### Frontend not connecting to backend

1. Verify CORS settings in App Service
2. Check API URL environment variable
3. Verify Static Web App custom domain configuration

## Cost Optimization

- Use Free/F1 tier for development
- Enable auto-pause for PostgreSQL in dev environment
- Monitor spending in Azure Cost Management

## Security Best Practices

- Rotate Key Vault secrets regularly
- Enable Azure AD authentication
- Use managed identities for Azure resource access
- Keep dependencies updated
