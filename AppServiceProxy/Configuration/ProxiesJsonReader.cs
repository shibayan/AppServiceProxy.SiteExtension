using System.Text.Json;

using static AppServiceProxy.Configuration.ProxiesJson;

namespace AppServiceProxy.Configuration
{
    internal class ProxiesJsonReader
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
                QueryString = requestOverrides.EnumerateObject().Where(x => x.Name.StartsWith("backend.request.querystring.")).ToDictionary(x => x.Name[28..], x => x.Value.GetString()!),
                Headers = requestOverrides.EnumerateObject().Where(x => x.Name.StartsWith("backend.request.headers.")).ToDictionary(x => x.Name[24..], x => x.Value.GetString()!)
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
                Headers = responseOverrides.EnumerateObject().Where(x => x.Name.StartsWith("response.headers.")).ToDictionary(x => x.Name[17..], x => x.Value.GetString()!)
            };

            return responseOverridesConfig;
        }
    }
}
