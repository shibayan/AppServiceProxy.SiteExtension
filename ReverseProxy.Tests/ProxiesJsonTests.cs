using Xunit;

namespace ReverseProxy.Tests
{
    public class ProxiesJsonTests
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>""
        }
    }
}
";

            var proxies = ProxiesJson.ParseJson(json);

            Assert.Equal(1, proxies.Count);
            Assert.Equal("proxy1", proxies[0].Name);
            Assert.False(proxies[0].Disabled);
            Assert.Equal(new[] { "GET" }, proxies[0].MatchCondition.Methods);
            Assert.Equal("/api/{test}", proxies[0].MatchCondition.Route);
            Assert.Equal("https://<AnotherApp>.azurewebsites.net/api/<FunctionName>", proxies[0].BackendUri);
            Assert.Null(proxies[0].RequestOverrides);
            Assert.Null(proxies[0].ResponseOverrides);
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>""
        }
    }
}
";

            var proxies = ProxiesJson.ParseJson(json);

            Assert.Equal(1, proxies.Count);
            Assert.Equal("Root", proxies[0].Name);
            Assert.True(proxies[0].Disabled);
            Assert.Equal(new[] { "GET" }, proxies[0].MatchCondition.Methods);
            Assert.Equal("/example", proxies[0].MatchCondition.Route);
            Assert.Equal("https://<AnotherApp>.azurewebsites.net/api/<FunctionName>", proxies[0].BackendUri);
            Assert.Null(proxies[0].RequestOverrides);
            Assert.Null(proxies[0].ResponseOverrides);
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>"",
            ""requestOverrides"": {
                ""backend.request.headers.Accept"": ""application/xml"",
                ""backend.request.headers.x-functions-key"": ""%ANOTHERAPP_API_KEY%""
            }
        }
    }
}
";

            var proxies = ProxiesJson.ParseJson(json);

            Assert.Equal(1, proxies.Count);
            Assert.Equal("proxy1", proxies[0].Name);
            Assert.False(proxies[0].Disabled);
            Assert.Equal(new[] { "GET"}, proxies[0].MatchCondition.Methods);
            Assert.Equal("/api/{test}", proxies[0].MatchCondition.Route);
            Assert.Equal("https://<AnotherApp>.azurewebsites.net/api/<FunctionName>", proxies[0].BackendUri);
            Assert.NotNull(proxies[0].RequestOverrides);
            Assert.Equal("application/xml", proxies[0].RequestOverrides.Headers["Accept"]);
            Assert.Equal("%ANOTHERAPP_API_KEY%", proxies[0].RequestOverrides.Headers["x-functions-key"]);
            Assert.Null(proxies[0].ResponseOverrides);
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>"",
            ""responseOverrides"": {
                ""response.statusCode"": 200,
                ""response.statusReason"": ""OK"",
                ""response.headers.Content-Type"": ""text/plain""
            }
        }
    }
}
";

            var proxies = ProxiesJson.ParseJson(json);

            Assert.Equal(1, proxies.Count);
            Assert.Equal("proxy1", proxies[0].Name);
            Assert.False(proxies[0].Disabled);
            Assert.Equal(new[] { "GET" }, proxies[0].MatchCondition.Methods);
            Assert.Equal("/api/{test}", proxies[0].MatchCondition.Route);
            Assert.Equal("https://<AnotherApp>.azurewebsites.net/api/<FunctionName>", proxies[0].BackendUri);
            Assert.Null(proxies[0].RequestOverrides);
            Assert.NotNull(proxies[0].ResponseOverrides);
            Assert.Equal(200, proxies[0].ResponseOverrides.StatusCode);
            Assert.Equal("OK", proxies[0].ResponseOverrides.StatusReason);
            Assert.Equal("text/plain", proxies[0].ResponseOverrides.Headers["Content-Type"]);
        }

        [Fact]
        public void Transform_Basic()
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/{test}""
        }
    }
}
";

            var proxies = ProxiesJson.ParseJson(json);

            var (routes, clusters) = ProxiesJson.Transform(proxies);

            Assert.Equal(1, routes.Count);
            Assert.Equal(1, clusters.Count);
            Assert.Equal($"Route_{proxies[0].Name}", routes[0].RouteId);
            Assert.Equal($"Cluster_{proxies[0].Name}", routes[0].ClusterId);
            Assert.Equal($"Cluster_{proxies[0].Name}", clusters[0].ClusterId);
            Assert.Equal(proxies[0].MatchCondition.Methods, routes[0].Match.Methods);
            Assert.Equal(proxies[0].MatchCondition.Route, routes[0].Match.Path);
            Assert.Equal("/api/{test}", routes[0].Transforms[0]["PathPattern"]);
            Assert.Equal("https://<AnotherApp>.azurewebsites.net", clusters[0].Destinations[$"Cluster_{proxies[0].Name}/destination"].Address);
        }

        [Fact]
        public void Transform_Disabled()
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>""
        }
    }
}
";

            var proxies = ProxiesJson.ParseJson(json);
            var (routes, clusters) = ProxiesJson.Transform(proxies);

            Assert.Equal(0, routes.Count);
            Assert.Equal(0, clusters.Count);
        }
    }
}
