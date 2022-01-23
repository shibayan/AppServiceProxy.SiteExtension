using System.Text.Json;

using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration
{
    internal class YarpJsonConfigFileLoader : IConfigFileLoader
    {
        public string ConfigFileName => "yarp.json";

        public (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) LoadConfig(string contents)
        {
            try
            {
                var yarp = JsonSerializer.Deserialize<YarpJson>(contents);

                return (yarp?.Routes ?? Array.Empty<RouteConfig>(), yarp?.Clusters ?? Array.Empty<ClusterConfig>());
            }
            catch
            {
                return (Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
            }
        }
    }
}
