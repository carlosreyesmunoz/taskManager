@description('Environment name (dev or prod)')
param environment string = 'dev'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Application name prefix')
param appName string = 'taskmanager'

@description('Administrator username for PostgreSQL server')
@secure()
param dbAdminUsername string

@description('Administrator password for PostgreSQL server')
@secure()
param dbAdminPassword string

// Variables
var resourcePrefix = '${appName}-${environment}'
var keyVaultName = 'kv-${appName}-${environment}-${substring(uniqueString(resourceGroup().id), 0, 6)}'
var dbServerName = '${resourcePrefix}-psql-${substring(uniqueString(resourceGroup().id), 0, 6)}'
var appServicePlanName = '${resourcePrefix}-asp'
var webAppName = '${resourcePrefix}-api'
var staticWebAppName = '${resourcePrefix}-web'
var applicationInsightsName = '${resourcePrefix}-ai'
var logAnalyticsWorkspaceName = '${resourcePrefix}-law'

// Deploy Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Deploy Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Deploy Key Vault
module keyVault 'modules/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    keyVaultName: keyVaultName
    location: location
    environment: environment
  }
}

// Deploy PostgreSQL Database
module database 'modules/database.bicep' = {
  name: 'databaseDeployment'
  params: {
    serverName: dbServerName
    location: location
    adminUsername: dbAdminUsername
    adminPassword: dbAdminPassword
    environment: environment
  }
}

// Deploy App Service
module appService 'modules/appservice.bicep' = {
  name: 'appServiceDeployment'
  params: {
    appServicePlanName: appServicePlanName
    webAppName: webAppName
    location: location
    environment: environment
    keyVaultName: keyVaultName
    dbConnectionString: 'Host=${database.outputs.serverFqdn};Database=${database.outputs.databaseName};Username=${dbAdminUsername};Password=${dbAdminPassword};SSL Mode=Require;Trust Server Certificate=true'
    applicationInsightsConnectionString: applicationInsights.properties.ConnectionString
  }
  dependsOn: [
    keyVault
    database
  ]
}

// Deploy Static Web App
module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'staticWebAppDeployment'
  params: {
    staticWebAppName: staticWebAppName
    location: location
    environment: environment
    apiUrl: appService.outputs.webAppUrl
  }
}

// Store secrets in Key Vault
resource dbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: '${keyVaultName}/DatabaseConnectionString'
  properties: {
    value: 'Host=${database.outputs.serverFqdn};Database=${database.outputs.databaseName};Username=${dbAdminUsername};Password=${dbAdminPassword};SSL Mode=Require;Trust Server Certificate=true'
  }
  dependsOn: [
    keyVault
    database
  ]
}

resource appInsightsConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: '${keyVaultName}/ApplicationInsightsConnectionString'
  properties: {
    value: applicationInsights.properties.ConnectionString
  }
  dependsOn: [
    keyVault
  ]
}

// Outputs
output keyVaultName string = keyVaultName
output databaseServerName string = dbServerName
output webAppName string = webAppName
output staticWebAppName string = staticWebAppName
output webAppUrl string = appService.outputs.webAppUrl
output staticWebAppUrl string = staticWebApp.outputs.staticWebAppUrl
