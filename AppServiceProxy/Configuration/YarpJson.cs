using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration
{
    internal class YarpJson
    {
        public IReadOnlyList<RouteConfig> Routes { get; set; } = null!;

        public IReadOnlyList<ClusterConfig> Clusters { get; set; } = null!;
    }
}
