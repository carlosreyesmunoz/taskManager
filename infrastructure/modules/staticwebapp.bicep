@description('Name of the Static Web App')
param staticWebAppName string

@description('Location for all resources')
param location string = 'West US 2'

@description('Environment name')
param environment string

@description('API URL for the backend')
param apiUrl string

@description('SKU for Static Web App')
param sku string = 'Free'

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: sku
    tier: sku
  }
  properties: {
    repositoryUrl: ''
    branch: ''
    buildProperties: {
      appLocation: '/frontend'
      apiLocation: ''
      outputLocation: 'dist'
    }
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
    provider: 'None'
  }
  tags: {
    Environment: environment
    Application: 'TaskManager'
  }
}

// Configure app settings for the Static Web App
resource staticWebAppSettings 'Microsoft.Web/staticSites/config@2023-12-01' = {
  name: 'appsettings'
  parent: staticWebApp
  properties: {
    REACT_APP_API_URL: apiUrl
    REACT_APP_ENVIRONMENT: environment
  }
}

output staticWebAppName string = staticWebApp.name
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey