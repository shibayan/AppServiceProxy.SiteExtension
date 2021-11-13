# App Service Proxy Site Extension
 
[![Build](https://github.com/shibayan/AppServiceProxy.SiteExtension/workflows/Build/badge.svg)](https://github.com/shibayan/AppServiceProxy.SiteExtension/actions/workflows/build.yml)
[![Downloads](https://badgen.net/nuget/dt/AppServiceProxy.SiteExtension)](https://www.nuget.org/packages/AppServiceProxy.SiteExtension/)
[![NuGet](https://badgen.net/nuget/v/AppServiceProxy.SiteExtension)](https://www.nuget.org/packages/AppServiceProxy.SiteExtension/)
[![License](https://badgen.net/github/license/shibayan/AppServiceProxy.SiteExtension)](https://github.com/shibayan/AppServiceProxy.SiteExtension/blob/master/LICENSE)

Site Extension-based Reverse Proxy compatible with Azure Functions Proxies

## Features

- .NET 6.0 and YARP-based reverse proxies
- Compatibility with Azure Functions Proxies (`proxies.json`)
- Easy to setup
- Support for Git integration and CI pipelines

## Installation

You need to specify the `App Service Proxy` or `AppServiceProxy.SiteExtension` for installation.

### Azure Portal

- [Tip 21 - Adding Extensions to Web Apps in Azure App Service | Azure Tips and Tricks](https://microsoft.github.io/AzureTipsAndTricks/blog/tip21.html)

### ARM Template

```json
{
  "apiVersion": "2020-06-01",
  "name": "AppServiceProxy.SiteExtension",
  "type": "siteextensions",
  "dependsOn": [
    "[resourceId('Microsoft.Web/sites', variables('webAppName'))]"
  ],
  "properties": {}
}
```

## Usage

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

## Appendix: `proxies.json` Reference

- [Advanced configuration - Work with proxies in Azure Functions | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-proxies#advanced-configuration)

## License

This project is licensed under the [MIT License](https://github.com/shibayan/AppServiceProxy.SiteExtension/blob/master/LICENSE)
