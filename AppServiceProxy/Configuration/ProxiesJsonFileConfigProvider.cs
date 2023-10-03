
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration;

internal class ProxiesJsonFileConfigProvider : IProxyConfigProvider
{
    private readonly PhysicalFileProvider _fileProvider = new(s_wwwroot)
    {
        UseActivePolling = true,
        UsePollingFileWatcher = true
    };

    private const string ProxiesJsonFileName = "proxies.json";

    private static readonly string s_wwwroot = Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot");

    public IProxyConfig GetConfig()
    {
        var proxiesJsonFile = Path.Combine(s_wwwroot, ProxiesJsonFileName);

        var changeToken = _fileProvider.Watch(ProxiesJsonFileName);

        return new ProxiesJsonFileConfig(proxiesJsonFile, changeToken);
    }

    private class ProxiesJsonFileConfig : IProxyConfig
    {
        public ProxiesJsonFileConfig(string proxiesJsonFile, IChangeToken changeToken)
        {
            try
            {
                var json = File.ReadAllText(proxiesJsonFile);

                var proxies = ProxiesJsonReader.ParseJson(json);
                var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

                Routes = routes;
                Clusters = clusters;
            }
            catch
            {
                Routes = Array.Empty<RouteConfig>();
                Clusters = Array.Empty<ClusterConfig>();
            }

            ChangeToken = changeToken;
        }

        public IReadOnlyList<RouteConfig> Routes { get; }

        public IReadOnlyList<ClusterConfig> Clusters { get; }

        public IChangeToken ChangeToken { get; }
    }
}
