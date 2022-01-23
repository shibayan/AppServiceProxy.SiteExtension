# App Service Proxy Site Extension
 
[![Build](https://github.com/shibayan/AppServiceProxy.SiteExtension/workflows/Build/badge.svg)](https://github.com/shibayan/AppServiceProxy.SiteExtension/actions/workflows/build.yml)
[![Downloads](https://badgen.net/nuget/dt/AppServiceProxy.SiteExtension)](https://www.nuget.org/packages/AppServiceProxy.SiteExtension/)
[![NuGet](https://badgen.net/nuget/v/AppServiceProxy.SiteExtension)](https://www.nuget.org/packages/AppServiceProxy.SiteExtension/)
[![License](https://badgen.net/github/license/shibayan/AppServiceProxy.SiteExtension)](https://github.com/shibayan/AppServiceProxy.SiteExtension/blob/master/LICENSE)
[![Terraform Registry](https://badgen.net/badge/terraform/registry/5c4ee5)](https://registry.terraform.io/modules/shibayan/appservice-proxy/azurerm/latest)

Site Extension-based Reverse Proxy compatible with Azure Functions Proxies

## Motivation

With the permanent removal of Azure Functions Proxies from Azure Functions v4, there is no longer a readily available and inexpensive L7 reverse proxy in Azure.

This project provides an alternative implementation of a VNET-integrated gateway built using Azure Functions Proxies, and secure proxies authenticated in combination with App Service Authentication.

## Features

- All features of the App Service is available
  - App Service Authentication
  - Custom Domain
  - SSL / TLS (Managed Certificate / Key Vault Certificate)
  - VNET Integration
- .NET 6.0 and YARP-based high-performance reverse proxy
- Compatibility with Azure Functions Proxies (`proxies.json`)
- Easy to setup with Azure Portal or ARM Template
- Support for Git integration and CI pipelines

## Quick Start

| Azure (Public) | Azure China | Azure Government |
| :---: | :---: | :---: |
| <a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fshibayan%2FAppServiceProxy.SiteExtension%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="https://aka.ms/deploytoazurebutton" /></a> | <a href="https://portal.azure.cn/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fshibayan%2FAppServiceProxy.SiteExtension%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="https://aka.ms/deploytoazurebutton" /></a> | <a href="https://portal.azure.us/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fshibayan%2FAppServiceProxy.SiteExtension%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="https://aka.ms/deploytoazurebutton" /></a> |

## Manual Installation

Since this application is based on .NET 6.0 and Site Extension, it requires a App Service (Windows) with .NET 6.0 enabled.

You need to specify the `App Service Proxy` or `AppServiceProxy.SiteExtension` for installation.

### Azure Portal

![site extensions](https://user-images.githubusercontent.com/1356444/141622631-7d5c71c8-da20-4353-a898-141fdda73814.png)

- [Tip 21 - Adding Extensions to Web Apps in Azure App Service | Azure Tips and Tricks](https://microsoft.github.io/AzureTipsAndTricks/blog/tip21.html)

### ARM Template

```json
{
  "apiVersion": "2021-02-01",
  "name": "AppServiceProxy.SiteExtension",
  "type": "siteextensions",
  "dependsOn": [
    "[resourceId('Microsoft.Web/sites', variables('webAppName'))]"
  ]
}
```

### Bicep

```bicep
resource symbolicname 'Microsoft.Web/sites/siteextensions@2021-02-01' = {
  name: '${webApp.name}/AppServiceProxy.SiteExtension'
}
```

### Terraform

See also [App Service Proxy Terraform module](https://github.com/shibayan/terraform-azurerm-appservice-proxy) repository.

## Usage

### YARP

Create `yarp.json` info `wwwroot` directory.

```json
{
  "Routes": {
    "route1" : {
      "ClusterId": "cluster1",
      "Match": {
        "Path": "{**catch-all}"
      }
    }
  },
  "Clusters": {
    "cluster1": {
      "Destinations": {
        "cluster1/destination1": {
          "Address": "https://shibayan.jp/"
        }
      }
    }
  }
}
```

- https://microsoft.github.io/reverse-proxy/articles/config-files.html

### Azure Functions Proxies

Create `proxies.json` into `wwwroot` directory.

```json
{
  "$schema": "http://json.schemastore.org/proxies",
  "proxies": {
    "proxy1": {
      "matchCondition": {
        "methods": [ "GET" ],
        "route": "/{*path}"
      },
      "backendUri": "https://shibayan.jp/{path}"
    }
  }
}
```

- [Advanced configuration - Work with proxies in Azure Functions | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies#advanced-configuration)

## License

This project is licensed under the [MIT License](https://github.com/shibayan/AppServiceProxy.SiteExtension/blob/master/LICENSE)
