using System.Text.Json;

using Yarp.ReverseProxy.Configuration;

namespace ReverseProxy
{
    internal class ProxiesJson
    {
        public static IReadOnlyList<ProxyConfig> ParseJson(string json)
        {
            var document = JsonDocument.Parse(json);

            var proxies = document.RootElement.GetProperty("proxies");

            var result = new List<ProxyConfig>();

            foreach (var proxy in proxies.EnumerateObject())
            {
                var proxyConfig = ParseProxyConfig(proxy);

                result.Add(proxyConfig);
            }

            return result;
        }

        public static (IReadOnlyList<RouteConfig>, IReadOnlyList<ClusterConfig>) Transform(IReadOnlyList<ProxyConfig> proxies)
        {
            var routes = new List<RouteConfig>();
            var clusters = new List<ClusterConfig>();

            foreach (var proxy in proxies.Where(x => !x.Disabled))
            {
                var pathIndex = proxy.BackendUri.IndexOf('/', 8);

                var destinationAddress = pathIndex == -1 ? proxy.BackendUri : proxy.BackendUri.Substring(0, pathIndex);
                var absolutePath = pathIndex == -1 ? null : proxy.BackendUri.Substring(pathIndex);

                var transforms = new List<Dictionary<string, string>>();

                if (!string.IsNullOrEmpty(absolutePath))
                {
                    transforms.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "PathPattern", absolutePath }
                    });
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
                        { $"Cluster_{proxy.Name}/destination1", new DestinationConfig { Address = destinationAddress } }
                    }
                };

                routes.Add(route);
                clusters.Add(cluster);
            }

            return (routes, clusters);
        }

        private static ProxyConfig ParseProxyConfig(JsonProperty proxy)
        {
            var matchCondition = proxy.Value.GetProperty("matchCondition");

            var matchConditionConfig = new MatchConditionConfig
            {
                Methods = matchCondition.GetProperty("methods").EnumerateArray().Select(x => x.GetString()!).ToArray(),
                Route = matchCondition.GetProperty("route").GetString()!
            };

            var proxyConfig = new ProxyConfig
            {
                Name = proxy.Name,
                Disabled = proxy.Value.TryGetProperty("disabled", out var value) && value.GetBoolean(),
                BackendUri = proxy.Value.GetProperty("backendUri").GetString()!,
                MatchCondition = matchConditionConfig,
                RequestOverrides = ParseRequestOverridesConfig(proxy),
                ResponseOverrides = ParseResponseOverridesConfig(proxy)
            };

            return proxyConfig;
        }

        private static RequestOverridesConfig? ParseRequestOverridesConfig(JsonProperty proxy)
        {
            if (!proxy.Value.TryGetProperty("requestOverrides", out var requestOverrides))
            {
                return null;
            }

            var requestOverridesConfig = new RequestOverridesConfig
            {
                Method = requestOverrides.TryGetProperty("method", out var method) ? method.GetString() : null,
                QueryString = requestOverrides.EnumerateObject().Where(x => x.Name.StartsWith("backend.request.querystring.")).ToDictionary(x => x.Name.Substring(28), x => x.Value.GetString()!),
                Headers = requestOverrides.EnumerateObject().Where(x => x.Name.StartsWith("backend.request.headers.")).ToDictionary(x => x.Name.Substring(24), x => x.Value.GetString()!)
            };

            return requestOverridesConfig;
        }

        private static ResponseOverridesConfig? ParseResponseOverridesConfig(JsonProperty proxy)
        {
            if (!proxy.Value.TryGetProperty("responseOverrides", out var responseOverrides))
            {
                return null;
            }

            var responseOverridesConfig = new ResponseOverridesConfig
            {
                StatusCode = responseOverrides.TryGetProperty("response.statusCode", out var statusCode) ? statusCode.GetInt32() : null,
                StatusReason = responseOverrides.TryGetProperty("response.statusReason", out var statusReason) ? statusReason.GetString() : null,
                Headers = responseOverrides.EnumerateObject().Where(x => x.Name.StartsWith("response.headers.")).ToDictionary(x => x.Name.Substring(17), x => x.Value.GetString()!)
            };

            return responseOverridesConfig;
        }

        internal class ProxyConfig
        {
            public string Name { get; set; }

            public bool Disabled { get; init; }

            public MatchConditionConfig MatchCondition { get; init; }

            public string BackendUri { get; init; }

            public RequestOverridesConfig? RequestOverrides { get; init; }

            public ResponseOverridesConfig? ResponseOverrides { get; init; }
        }

        internal class MatchConditionConfig
        {
            public IReadOnlyList<string> Methods { get; init; }

            public string Route { get; init; }
        }

        internal class RequestOverridesConfig
        {
            public string? Method { get; init; }
            public IReadOnlyDictionary<string, string> QueryString { get; init; }
            public IReadOnlyDictionary<string, string> Headers { get; init; }
        }

        internal class ResponseOverridesConfig
        {
            public int? StatusCode { get; init; }
            public string? StatusReason { get; init; }
            public IReadOnlyDictionary<string, string> Headers { get; init; }
        }
    }
}
