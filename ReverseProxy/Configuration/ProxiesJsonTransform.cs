using Yarp.ReverseProxy.Configuration;

using static ReverseProxy.Configuration.ProxiesJson;

namespace ReverseProxy.Configuration
{
    internal static class ProxiesJsonTransform
    {
        public static (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) Apply(IReadOnlyList<ProxyConfig> proxies)
        {
            var routes = new List<RouteConfig>();
            var clusters = new List<ClusterConfig>();

            foreach (var proxy in proxies.Where(x => !x.Disabled))
            {
                var backendUri = ExpandValue(proxy.BackendUri);

                var (destinationAddress, absolutePath) = SplitBackendUri(backendUri);

                var transforms = new List<Dictionary<string, string>>();

                if (!string.IsNullOrEmpty(absolutePath))
                {
                    transforms.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "PathPattern", absolutePath }
                    });
                }

                if (proxy.RequestOverrides != null)
                {
                    foreach (var header in proxy.RequestOverrides.Headers)
                    {
                        transforms.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "RequestHeader", header.Key },
                            { "Append", ExpandValue(header.Value) }
                        });
                    }
                }

                if (proxy.ResponseOverrides != null)
                {
                    foreach (var header in proxy.ResponseOverrides.Headers)
                    {
                        transforms.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "ResponseHeader", header.Key },
                            { "Append", ExpandValue(header.Value) }
                        });
                    }
                }

                var route = new RouteConfig
                {
                    RouteId = $"Route_{proxy.Name}",
                    ClusterId = $"Cluster_{proxy.Name}",
                    Match = new RouteMatch
                    {
                        Methods = proxy.MatchCondition.Methods,
                        Path = proxy.MatchCondition.Route
                    },
                    Transforms = transforms
                };

                var cluster = new ClusterConfig
                {
                    ClusterId = $"Cluster_{proxy.Name}",
                    Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                    {
                        { $"Cluster_{proxy.Name}/destination", new DestinationConfig { Address = destinationAddress } }
                    }
                };

                routes.Add(route);
                clusters.Add(cluster);
            }

            return (routes, clusters);
        }

        private static string ExpandValue(string value) => Environment.ExpandEnvironmentVariables(value);

        private static (string, string?) SplitBackendUri(string backendUri)
        {
            var pathIndex = backendUri.IndexOf('/', 8);

            var destinationAddress = pathIndex == -1 ? backendUri : backendUri[..pathIndex];
            var absolutePath = pathIndex == -1 ? null : backendUri[pathIndex..];

            return (destinationAddress, absolutePath);
        }
    }
}
