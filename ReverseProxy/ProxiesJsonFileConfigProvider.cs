using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

namespace ReverseProxy
{
    public class ProxiesJsonFileConfigProvider : IProxyConfigProvider
    {
        public ProxiesJsonFileConfigProvider()
        {
            var wwwroot = Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot");
            var proxiesJsonFile = Path.Combine(wwwroot, ProxiesJsonFileName);

            _config = new ProxiesJsonFileConfig(proxiesJsonFile);
        }

        private const string ProxiesJsonFileName = "proxies.json";

        private readonly ProxiesJsonFileConfig _config;

        public IProxyConfig GetConfig() => _config;

        private class ProxiesJsonFileConfig : IProxyConfig
        {
            public ProxiesJsonFileConfig(string proxiesJsonFile)
            {
                var json = File.ReadAllText(proxiesJsonFile);
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }
        }
    }
}
