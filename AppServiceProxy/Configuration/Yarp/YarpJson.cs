
using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration.Yarp;

internal class YarpJson
{
    public IReadOnlyList<RouteConfig> Routes { get; set; } = null!;

    public IReadOnlyList<ClusterConfig> Clusters { get; set; } = null!;
}
