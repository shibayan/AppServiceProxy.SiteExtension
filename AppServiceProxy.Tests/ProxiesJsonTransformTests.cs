using System;

using AppServiceProxy.Configuration;

using Xunit;

namespace AppServiceProxy.Tests;

public class ProxiesJsonTransformTests
{
    [Fact]
    public void Basic()
    {
        const string json = @"
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

        Assert.Single(routes);
        Assert.Single(clusters);

        Assert.Equal($"Route_{proxies[0].Name}", routes[0].RouteId);
        Assert.Equal($"Cluster_{proxies[0].Name}", routes[0].ClusterId);
        Assert.Equal($"Cluster_{proxies[0].Name}", clusters[0].ClusterId);
        Assert.Equal(proxies[0].MatchCondition.Methods, routes[0].Match.Methods);
        Assert.Equal(proxies[0].MatchCondition.Route, routes[0].Match.Path);

        Assert.NotNull(routes[0].Transforms);
        Assert.Single(routes[0].Transforms!);

        Assert.Equal("/backend/{test}", routes[0].Transforms![0]["PathPattern"]);
        Assert.Equal("https://<AnotherApp>.azurewebsites.net", clusters[0].Destinations![$"Cluster_{proxies[0].Name}/destination"].Address);
    }

    [Fact]
    public void Disabled()
    {
        const string json = @"
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

        Assert.Empty(routes);
        Assert.Empty(clusters);
    }

    [Fact]
    public void RequestOverrides()
    {
        const string json = @"
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

        Assert.Single(routes);
        Assert.Single(clusters);

        Assert.NotNull(routes[0].Transforms);
        Assert.Equal(2, routes[0].Transforms!.Count);

        Assert.Equal("/backend/{test}", routes[0].Transforms![0]["PathPattern"]);
        Assert.Equal("Accept", routes[0].Transforms![1]["RequestHeader"]);
        Assert.Equal("application/xml", routes[0].Transforms![1]["Append"]);
    }

    [Fact]
    public void ResponseOverrides()
    {
        const string json = @"
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

        Assert.Single(routes);
        Assert.Single(clusters);

        Assert.NotNull(routes[0].Transforms);
        Assert.Equal(2, routes[0].Transforms!.Count);

        Assert.Equal("/backend/{test}", routes[0].Transforms![0]["PathPattern"]);
        Assert.Equal("Content-Type", routes[0].Transforms![1]["ResponseHeader"]);
        Assert.Equal("text/plain", routes[0].Transforms![1]["Append"]);
    }

    [Fact]
    public void CatchAll()
    {
        const string json = @"
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

        Assert.Single(routes);
        Assert.Single(clusters);

        Assert.Equal("/{**path}", routes[0].Match.Path);

        Assert.NotNull(routes[0].Transforms);
        Assert.Single(routes[0].Transforms!);

        Assert.Equal("/backend/{**path}", routes[0].Transforms![0]["PathPattern"]);
    }

    [Fact]
    public void EnvironmentVariable()
    {
        Environment.SetEnvironmentVariable("APP_HOST_NAME", "example.com");
        Environment.SetEnvironmentVariable("ANOTHERAPP_API_KEY", "api-key", EnvironmentVariableTarget.Process);

        const string json = @"
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

        Assert.Single(routes);
        Assert.Single(clusters);

        Assert.NotNull(routes[0].Transforms);
        Assert.Equal(2, routes[0].Transforms!.Count);

        Assert.Equal("/backend/{test}", routes[0].Transforms![0]["PathPattern"]);
        Assert.Equal("x-functions-key", routes[0].Transforms![1]["RequestHeader"]);
        Assert.Equal("api-key", routes[0].Transforms![1]["Append"]);

        Assert.NotNull(clusters[0].Destinations);
        Assert.Single(clusters[0].Destinations!);

        Assert.Equal("https://example.com", clusters[0].Destinations![$"Cluster_{proxies[0].Name}/destination"].Address);
    }
}
