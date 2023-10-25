using System.Text.Json;

using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration.Yarp;

internal class YarpJsonReader
{
    public static YarpJson ParseJson(string json)
    {
        var document = JsonDocument.Parse(json);

        var routes = document.RootElement.GetProperty("Routes");
        var clusters = document.RootElement.GetProperty("Clusters");

        return new YarpJson { };
    }

    private static RouteConfig ParseRouteConfig(JsonProperty route)
    {
        return new RouteConfig
        {
            RouteId = route.Name,
            Order = route.Value.TryGetProperty(nameof(RouteConfig.Order), out var order) ? order.GetInt32() : null,
            ClusterId = route.Value.TryGetProperty(nameof(RouteConfig.ClusterId), out var clusterId) ? clusterId.GetString() : null,
            AuthorizationPolicy = route.Value.TryGetProperty(nameof(RouteConfig.AuthorizationPolicy), out var authorizationPolicy) ? authorizationPolicy.GetString() : null,
            CorsPolicy = route.Value.TryGetProperty(nameof(RouteConfig.CorsPolicy), out var corsPolicy) ? corsPolicy.GetString() : null,
            Metadata = route.Value.TryGetProperty(nameof(RouteConfig.Metadata), out var metadata) ? metadata.Deserialize<Dictionary<string, string>>() : null,
            Transforms = route.Value.TryGetProperty(nameof(RouteConfig.Transforms), out var transforms) ? ParseTransforms(transforms) : null
        };
    }

    private static ClusterConfig ParseClusterConfig(JsonProperty cluster)
    {
        return new ClusterConfig
        {
            ClusterId = cluster.Name,
            LoadBalancingPolicy = cluster.Value.TryGetProperty(nameof(ClusterConfig.LoadBalancingPolicy), out var loadBalancingPolicy) ? loadBalancingPolicy.GetString() : null,
        };
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string>>? ParseTransforms(JsonElement transforms)
    {
        if (transforms.GetArrayLength() == 0)
        {
            return null;
        }

        return transforms.EnumerateArray()
                         .Select(x => x.EnumerateObject().ToDictionary(xs => xs.Name, xs => xs.Value.GetString()!, StringComparer.OrdinalIgnoreCase))
                         .ToList();
    }
}
