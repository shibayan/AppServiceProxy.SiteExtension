using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

namespace ReverseProxy.Configuration
{
    internal class ProxiesJsonFileConfigProvider : IProxyConfigProvider
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

                var proxies = ProxiesJsonReader.ParseJson(json);
                var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

                Routes = routes;
                Clusters = clusters;
                ChangeToken = new PollingFileChangeToken(new FileInfo(proxiesJsonFile));
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }
        }
    }
}
