@description('The name of the function app that you wish to create.')
@maxLength(14)
param appNamePrefix string

@description('The location of the function app that you wish to create.')
param location string = resourceGroup().location

@description('The SKU of App Service Plan.')
@allowed([
  'F1'
  'B1'
  'S1'
  'P1v2'
  'P1v3'
])
param sku string

var webAppName = 'app-${appNamePrefix}-${substring(uniqueString(resourceGroup().id, deployment().name), 0, 4)}'
var appServicePlanName = 'plan-${appNamePrefix}-${substring(uniqueString(resourceGroup().id, deployment().name), 0, 4)}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: sku
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: webAppName
  location: location
  properties: {
    clientAffinityEnabled: false
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v7.0'
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
    }
  }
}

resource webAppName_AppServiceProxy_SiteExtension 'Microsoft.Web/sites/siteextensions@2022-03-01' = {
  parent: webApp
  name: 'AppServiceProxy.SiteExtension'
}
