using System.Diagnostics.CodeAnalysis;

using AppServiceProxy.Configuration.Yarp;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration;

internal class FileBaseProxyConfigProvider : IProxyConfigProvider
{
    public FileBaseProxyConfigProvider(IEnumerable<IConfigFileLoader> configFileLoaders)
    {
        _configFileLoaders = configFileLoaders;

        var json = System.Text.Json.JsonSerializer.Deserialize<YarpJson>(File.ReadAllText(@"C:\Users\shibayan\Documents\yarp.json"));

        _fileProvider = new PhysicalFileProvider(s_wwwroot)
        {
            UseActivePolling = true,
            UsePollingFileWatcher = true
        };
    }

    private readonly IEnumerable<IConfigFileLoader> _configFileLoaders;
    private readonly PhysicalFileProvider _fileProvider;

    private static readonly string s_wwwroot = Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot");

    public IProxyConfig GetConfig()
    {
        if (TryGetConfigFileLoader(out var configFileLoader))
        {
            var contents = File.ReadAllText(Path.Combine(s_wwwroot, configFileLoader.ConfigFileName));

            var (routes, clusters) = configFileLoader.LoadConfig(contents);

            var changeToken = _fileProvider.Watch(configFileLoader.ConfigFileName);

            return new FileBaseProxyConfig
            {
                Routes = routes,
                Clusters = clusters,
                ChangeToken = changeToken
            };
        }
        else
        {
            var changeToken = _fileProvider.Watch("*.*");

            return new FileBaseProxyConfig
            {
                Routes = Array.Empty<RouteConfig>(),
                Clusters = Array.Empty<ClusterConfig>(),
                ChangeToken = changeToken
            };
        }
    }

    private bool TryGetConfigFileLoader([NotNullWhen(true)] out IConfigFileLoader? configFileLoader)
    {
        configFileLoader = _configFileLoaders.FirstOrDefault(x => File.Exists(Path.Combine(s_wwwroot, x.ConfigFileName)));

        return configFileLoader is not null;
    }

    private class FileBaseProxyConfig : IProxyConfig
    {
        public IReadOnlyList<RouteConfig> Routes { get; init; } = null!;

        public IReadOnlyList<ClusterConfig> Clusters { get; init; } = null!;

        public IChangeToken ChangeToken { get; init; } = null!;
    }
}
