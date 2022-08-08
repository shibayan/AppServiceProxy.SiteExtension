
using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration.Yarp;

internal class YarpJsonConfigFileLoader : IConfigFileLoader
{
    public string ConfigFileName => "yarp.json";

    public (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) LoadConfig(string contents)
    {
        try
        {
            var yarp = YarpJsonReader.ParseJson(contents);

            return (yarp.Routes, yarp.Clusters);
        }
        catch
        {
            return (Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
        }
    }
}
