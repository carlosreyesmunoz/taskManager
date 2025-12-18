@description('Name of the PostgreSQL server')
param serverName string

@description('Location for all resources')
param location string

@description('Administrator username')
param adminUsername string

@description('Administrator password')
@secure()
param adminPassword string

@description('Environment name')
param environment string

@description('PostgreSQL version')
param postgresVersion string = '15'

@description('Server SKU')
param skuName string = environment == 'prod' ? 'Standard_B1ms' : 'Standard_B1ms'

@description('Server tier')
param tier string = environment == 'prod' ? 'Burstable' : 'Burstable'

@description('Storage size in GB')
param storageSizeGB int = 32

resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: serverName
  location: location
  sku: {
    name: skuName
    tier: tier
  }
  properties: {
    version: postgresVersion
    administratorLogin: adminUsername
    administratorLoginPassword: adminPassword
    storage: {
      storageSizeGB: storageSizeGB
      tier: 'P4'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
  tags: {
    Environment: environment
    Application: 'TaskManager'
  }
}

// Allow Azure services to access the server
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = {
  name: 'AllowAzureServices'
  parent: postgresqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Allow all IPs for development (remove in production)
resource allowAllIPs 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = if (environment == 'dev') {
  name: 'AllowAllIPs'
  parent: postgresqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Create the application database
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  name: 'TaskManagerDb'
  parent: postgresqlServer
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

output serverName string = postgresqlServer.name
output serverFqdn string = postgresqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
// Connection string without password - password should be stored in Key Vault
output connectionStringTemplate string = 'Host=${postgresqlServer.properties.fullyQualifiedDomainName};Database=${database.name};Username=${adminUsername};Password=<from-keyvault>;SSL Mode=Require;Trust Server Certificate=true'
