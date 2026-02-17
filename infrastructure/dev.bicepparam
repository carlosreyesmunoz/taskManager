using 'main.bicep'

param environment = 'dev'
param location = 'swedencentral'
param appName = 'taskmanager'

// Azure AD admin parameters will be passed from GitHub Actions secrets
// These are placeholders and will be overridden by the deployment command
param azureAdAdminLogin = ''
param azureAdAdminSid = ''
param azureAdTenantId = ''
