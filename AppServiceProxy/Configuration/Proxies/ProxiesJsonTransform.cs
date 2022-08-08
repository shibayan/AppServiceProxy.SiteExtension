using System.Text.RegularExpressions;

using Yarp.ReverseProxy.Configuration;

using static AppServiceProxy.Configuration.Proxies.ProxiesJson;

namespace AppServiceProxy.Configuration.Proxies;

internal static class ProxiesJsonTransform
{
    private static readonly Regex s_templateRegex = new(@"\{([^\{\}]+)\}", RegexOptions.Compiled);

    public static (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) Apply(IReadOnlyList<ProxyConfig> proxies)
    {
        var routes = new List<RouteConfig>();
        var clusters = new List<ClusterConfig>();

        foreach (var proxy in proxies.Where(x => !x.Disabled))
        {
            var (route, backendUri) = TransformRouteParameters(proxy.MatchCondition.Route, ExpandValue(proxy.BackendUri));

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

            var routeConfig = new RouteConfig
            {
                RouteId = $"Route_{proxy.Name}",
                ClusterId = $"Cluster_{proxy.Name}",
                Match = new RouteMatch
                {
                    Methods = proxy.MatchCondition.Methods,
                    Path = route
                },
                Transforms = transforms
            };

            var clusterConfig = new ClusterConfig
            {
                ClusterId = $"Cluster_{proxy.Name}",
                Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                {
                    { $"Cluster_{proxy.Name}/destination", new DestinationConfig { Address = destinationAddress } }
                }
            };

            routes.Add(routeConfig);
            clusters.Add(clusterConfig);
        }

        return (routes, clusters);
    }

    private static string ExpandValue(string value) => Environment.ExpandEnvironmentVariables(value);

    private static (string, string) TransformRouteParameters(string route, string backendUri)
    {
        var catchAllParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var transformedRoute = s_templateRegex.Replace(route, m =>
        {
            var parameterName = m.Groups[1].Value;

            // Catch all
            if (parameterName.StartsWith("*"))
            {
                parameterName = parameterName[1..];

                catchAllParameters.Add(parameterName);

                return $"{{**{parameterName}}}";
            }

            return $"{{{parameterName}}}";
        });

        var transformedBackendUri = s_templateRegex.Replace(backendUri, m =>
        {
            var parameterName = m.Groups[1].Value;

            // Catch all
            return catchAllParameters.Contains(parameterName) ? $"{{**{parameterName}}}" : $"{{{parameterName}}}";
        });

        return (transformedRoute, transformedBackendUri);
    }

    private static (string, string?) SplitBackendUri(string backendUri)
    {
        var pathIndex = backendUri.IndexOf('/', 8);

        var destinationAddress = pathIndex == -1 ? backendUri : backendUri[..pathIndex];
        var absolutePath = pathIndex == -1 ? null : backendUri[pathIndex..];

        return (destinationAddress, absolutePath);
    }
}
