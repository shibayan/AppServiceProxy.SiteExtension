
using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration.Proxies;

internal class ProxiesJsonConfigFileLoader : IConfigFileLoader
{
    public string ConfigFileName => "proxies.json";

    public (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) LoadConfig(string contents)
    {
        try
        {
            var proxies = ProxiesJsonReader.ParseJson(contents);
            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            return (routes, clusters);
        }
        catch
        {
            return (Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
        }
    }
}
