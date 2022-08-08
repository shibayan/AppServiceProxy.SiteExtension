using AppServiceProxy.Configuration;
using AppServiceProxy.Configuration.Proxies;
using AppServiceProxy.Configuration.Yarp;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy();

builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigFileLoader, ProxiesJsonConfigFileLoader>());
builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigFileLoader, YarpJsonConfigFileLoader>());

builder.Services.AddSingleton<IProxyConfigProvider, FileBaseProxyConfigProvider>();

var app = builder.Build();

app.MapReverseProxy();

app.Run();
