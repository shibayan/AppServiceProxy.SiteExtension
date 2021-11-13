using AppServiceProxy.Configuration;

using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy();

builder.Services.AddSingleton<IProxyConfigProvider, ProxiesJsonFileConfigProvider>();

var app = builder.Build();

app.MapReverseProxy();

app.Run();
