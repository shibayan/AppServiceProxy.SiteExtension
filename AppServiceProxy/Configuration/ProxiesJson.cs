namespace AppServiceProxy.Configuration;

internal class ProxiesJson
{
    internal class ProxyConfig
    {
        public string Name { get; init; } = null!;

        public bool Disabled { get; init; }

        public MatchConditionConfig MatchCondition { get; init; } = null!;

        public string BackendUri { get; init; } = null!;

        public RequestOverridesConfig? RequestOverrides { get; init; }

        public ResponseOverridesConfig? ResponseOverrides { get; init; }
    }

    internal class MatchConditionConfig
    {
        public IReadOnlyList<string> Methods { get; init; } = null!;

        public string Route { get; init; } = null!;
    }

    internal class RequestOverridesConfig
    {
        public string? Method { get; init; }
        public IReadOnlyDictionary<string, string> QueryString { get; init; } = null!;
        public IReadOnlyDictionary<string, string> Headers { get; init; } = null!;
    }

    internal class ResponseOverridesConfig
    {
        public int? StatusCode { get; init; }
        public string? StatusReason { get; init; }
        public IReadOnlyDictionary<string, string> Headers { get; init; } = null!;
    }
}
