namespace ReverseProxy.Configuration
{
    internal class ProxiesJson
    {
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
