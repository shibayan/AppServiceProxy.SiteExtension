using System.Text.Json;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Yarp.ReverseProxy.Configuration;

namespace AppServiceProxy.Configuration
{
    internal class YarpJsonFileConfigProvider : IProxyConfigProvider
    {
        public YarpJsonFileConfigProvider()
        {
            _fileProvider = new PhysicalFileProvider(_wwwroot)
            {
                UseActivePolling = true,
                UsePollingFileWatcher = true
            };
        }

        private readonly PhysicalFileProvider _fileProvider;

        private const string YarpJsonFileName = "yarp.json";

        private static readonly string _wwwroot = Environment.ExpandEnvironmentVariables(@"%HOME%\site\wwwroot");

        public IProxyConfig GetConfig()
        {
            var yarpJsonFile = Path.Combine(_wwwroot, YarpJsonFileName);

            var changeToken = _fileProvider.Watch(yarpJsonFile);

            return new YarpJsonFileConfig(yarpJsonFile, changeToken);
        }

        private class YarpJsonFileConfig : IProxyConfig
        {
            public YarpJsonFileConfig(string yarpJsonFile, IChangeToken changeToken)
            {
                try
                {
                    var json = File.ReadAllText(yarpJsonFile);

                    var yarp = JsonSerializer.Deserialize<YarpJson>(json);

                    Routes = yarp?.Routes ?? Array.Empty<RouteConfig>();
                    Clusters = yarp?.Clusters ?? Array.Empty<ClusterConfig>(); ;
                }
                catch
                {
                    Routes = Array.Empty<RouteConfig>();
                    Clusters = Array.Empty<ClusterConfig>();
                }

                ChangeToken = changeToken;
            }

            public IReadOnlyList<RouteConfig> Routes { get; }

            public IReadOnlyList<ClusterConfig> Clusters { get; }

            public IChangeToken ChangeToken { get; }
        }
    }
}
