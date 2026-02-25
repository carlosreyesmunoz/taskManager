@description('Name of the SQL Server')
param serverName string

@description('Location for all resources')
param location string

@description('Environment name')
param environment string

@description('Database name')
param databaseName string = 'TaskManagerDb'

@description('Azure AD admin login (UPN, e.g. user@tenant.onmicrosoft.com)')
param azureAdAdminLogin string

@description('Azure AD admin SID (Object ID)')
param azureAdAdminSid string

@description('Azure AD Tenant ID')
param azureAdTenantId string

// Azure SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: serverName
  location: location
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      principalType: 'User'
      login: azureAdAdminLogin
      sid: azureAdAdminSid
      tenantId: azureAdTenantId
    }
  }
  tags: {
    Environment: environment
    Application: 'TaskManager'
  }
}

// Azure SQL Database - Free tier (serverless)
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  name: databaseName
  parent: sqlServer
  location: location
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368 // 32 GB
    autoPauseDelay: 60 // Auto-pause after 60 minutes of inactivity
    minCapacity: json('0.5')
    requestedBackupStorageRedundancy: 'Local'
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
  }
  tags: {
    Environment: environment
    Application: 'TaskManager'
  }
}

// Allow Azure services to access the server
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  name: 'AllowAzureServices'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Allow all IPs for development (remove in production)
resource allowAllIPs 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = if (environment == 'dev') {
  name: 'AllowAllIPs'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

output serverName string = sqlServer.name
output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = sqlDatabase.name
output connectionStringTemplate string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;Password=<from-keyvault>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
