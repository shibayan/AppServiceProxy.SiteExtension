using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration;

internal interface IConfigFileLoader
{
    string ConfigFileName { get; }

    (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) LoadConfig(string contents);
}
