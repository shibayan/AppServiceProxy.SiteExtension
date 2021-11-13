using System;

using AppServiceProxy.Configuration;

using Xunit;

namespace AppServiceProxy.Tests
{
    public class ProxiesJsonTransformTests
    {
        [Fact]
        public void Basic()
        {
            var json = @"
{
    ""$schema"": ""http://json.schemastore.org/proxies"",
    ""proxies"": {
        ""proxy1"": {
            ""matchCondition"": {
                ""methods"": [ ""GET"" ],
                ""route"": ""/api/{test}""
            },
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/backend/{test}""
        }
    }
}
";

            var proxies = ProxiesJsonReader.ParseJson(json);

            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            Assert.Equal(1, routes.Count);
            Assert.Equal(1, clusters.Count);

            Assert.Equal($"Route_{proxies[0].Name}", routes[0].RouteId);
            Assert.Equal($"Cluster_{proxies[0].Name}", routes[0].ClusterId);
            Assert.Equal($"Cluster_{proxies[0].Name}", clusters[0].ClusterId);
            Assert.Equal(proxies[0].MatchCondition.Methods, routes[0].Match.Methods);
            Assert.Equal(proxies[0].MatchCondition.Route, routes[0].Match.Path);

            Assert.NotNull(routes[0].Transforms);
            Assert.Equal(1, routes[0].Transforms.Count);

            Assert.Equal("/backend/{test}", routes[0].Transforms[0]["PathPattern"]);
            Assert.Equal("https://<AnotherApp>.azurewebsites.net", clusters[0].Destinations[$"Cluster_{proxies[0].Name}/destination"].Address);
        }

        [Fact]
        public void Disabled()
        {
            var json = @"
{
    ""$schema"": ""http://json.schemastore.org/proxies"",
    ""proxies"": {
        ""Root"": {
            ""disabled"":true,
            ""matchCondition"": {
                ""methods"": [ ""GET"" ],
                ""route"": ""/example""
            },
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/backend/<FunctionName>""
        }
    }
}
";

            var proxies = ProxiesJsonReader.ParseJson(json);
            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            Assert.Equal(0, routes.Count);
            Assert.Equal(0, clusters.Count);
        }

        [Fact]
        public void RequestOverrides()
        {
            var json = @"
{
    ""$schema"": ""http://json.schemastore.org/proxies"",
    ""proxies"": {
        ""proxy1"": {
            ""matchCondition"": {
                ""methods"": [ ""GET"" ],
                ""route"": ""/api/{test}""
            },
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/backend/{test}"",
            ""requestOverrides"": {
                ""backend.request.headers.Accept"": ""application/xml""
            }
        }
    }
}
";

            var proxies = ProxiesJsonReader.ParseJson(json);

            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            Assert.Equal(1, routes.Count);
            Assert.Equal(1, clusters.Count);

            Assert.NotNull(routes[0].Transforms);
            Assert.Equal(2, routes[0].Transforms.Count);

            Assert.Equal("/backend/{test}", routes[0].Transforms[0]["PathPattern"]);
            Assert.Equal("Accept", routes[0].Transforms[1]["RequestHeader"]);
            Assert.Equal("application/xml", routes[0].Transforms[1]["Append"]);
        }

        [Fact]
        public void ResponseOverrides()
        {
            var json = @"
{
    ""$schema"": ""http://json.schemastore.org/proxies"",
    ""proxies"": {
        ""proxy1"": {
            ""matchCondition"": {
                ""methods"": [ ""GET"" ],
                ""route"": ""/api/{test}""
            },
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/backend/{test}"",
            ""responseOverrides"": {
                ""response.statusCode"": 200,
                ""response.statusReason"": ""OK"",
                ""response.headers.Content-Type"": ""text/plain""
            }
        }
    }
}
";

            var proxies = ProxiesJsonReader.ParseJson(json);

            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            Assert.Equal(1, routes.Count);
            Assert.Equal(1, clusters.Count);

            Assert.NotNull(routes[0].Transforms);
            Assert.Equal(2, routes[0].Transforms.Count);

            Assert.Equal("/backend/{test}", routes[0].Transforms[0]["PathPattern"]);
            Assert.Equal("Content-Type", routes[0].Transforms[1]["ResponseHeader"]);
            Assert.Equal("text/plain", routes[0].Transforms[1]["Append"]);
        }

        [Fact]
        public void CatchAll()
        {
            var json = @"
{
    ""$schema"": ""http://json.schemastore.org/proxies"",
    ""proxies"": {
        ""proxy1"": {
            ""matchCondition"": {
                ""methods"": [ ""GET"" ],
                ""route"": ""/{*path}""
            },
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/backend/{path}""
        }
    }
}
";

            var proxies = ProxiesJsonReader.ParseJson(json);

            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            Assert.Equal(1, routes.Count);
            Assert.Equal(1, clusters.Count);

            Assert.Equal("/{**path}", routes[0].Match.Path);

            Assert.NotNull(routes[0].Transforms);
            Assert.Equal(1, routes[0].Transforms.Count);

            Assert.Equal("/backend/{**path}", routes[0].Transforms[0]["PathPattern"]);
        }

        [Fact]
        public void EnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("APP_HOST_NAME", "example.com");
            Environment.SetEnvironmentVariable("ANOTHERAPP_API_KEY", "api-key", EnvironmentVariableTarget.Process);

            var json = @"
{
    ""$schema"": ""http://json.schemastore.org/proxies"",
    ""proxies"": {
        ""proxy1"": {
            ""matchCondition"": {
                ""methods"": [ ""GET"" ],
                ""route"": ""/api/{test}""
            },
            ""backendUri"": ""https://%APP_HOST_NAME%/backend/{test}"",
            ""requestOverrides"": {
                ""backend.request.headers.x-functions-key"": ""%ANOTHERAPP_API_KEY%""
            }
        }
    }
}
";

            var proxies = ProxiesJsonReader.ParseJson(json);

            var (routes, clusters) = ProxiesJsonTransform.Apply(proxies);

            Assert.Equal(1, routes.Count);
            Assert.Equal(1, clusters.Count);

            Assert.NotNull(routes[0].Transforms);
            Assert.Equal(2, routes[0].Transforms.Count);

            Assert.Equal("/backend/{test}", routes[0].Transforms[0]["PathPattern"]);
            Assert.Equal("x-functions-key", routes[0].Transforms[1]["RequestHeader"]);
            Assert.Equal("api-key", routes[0].Transforms[1]["Append"]);

            Assert.NotNull(clusters[0].Destinations);
            Assert.Equal(1, clusters[0].Destinations.Count);

            Assert.Equal("https://example.com", clusters[0].Destinations[$"Cluster_{proxies[0].Name}/destination"].Address);
        }
    }
}
