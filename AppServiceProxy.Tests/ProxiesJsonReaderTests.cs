using AppServiceProxy.Configuration;

using Xunit;

namespace AppServiceProxy.Tests;

public class ProxiesJsonReaderTests
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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>""
        }
    }
}
";

        var proxies = ProxiesJsonReader.ParseJson(json);

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
            ""backendUri"": ""https://<AnotherApp>.azurewebsites.net/api/<FunctionName>""
        }
    }
}
";

        var proxies = ProxiesJsonReader.ParseJson(json);

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
        const string json = @"
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

        var proxies = ProxiesJsonReader.ParseJson(json);

        Assert.Equal(1, proxies.Count);
        Assert.Equal("proxy1", proxies[0].Name);
        Assert.False(proxies[0].Disabled);
        Assert.Equal(new[] { "GET" }, proxies[0].MatchCondition.Methods);
        Assert.Equal("/api/{test}", proxies[0].MatchCondition.Route);
        Assert.Equal("https://<AnotherApp>.azurewebsites.net/api/<FunctionName>", proxies[0].BackendUri);
        Assert.NotNull(proxies[0].RequestOverrides);
        Assert.Equal("application/xml", proxies[0].RequestOverrides!.Headers["Accept"]);
        Assert.Equal("%ANOTHERAPP_API_KEY%", proxies[0].RequestOverrides!.Headers["x-functions-key"]);
        Assert.Null(proxies[0].ResponseOverrides);
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

        var proxies = ProxiesJsonReader.ParseJson(json);

        Assert.Equal(1, proxies.Count);
        Assert.Equal("proxy1", proxies[0].Name);
        Assert.False(proxies[0].Disabled);
        Assert.Equal(new[] { "GET" }, proxies[0].MatchCondition.Methods);
        Assert.Equal("/api/{test}", proxies[0].MatchCondition.Route);
        Assert.Equal("https://<AnotherApp>.azurewebsites.net/api/<FunctionName>", proxies[0].BackendUri);
        Assert.Null(proxies[0].RequestOverrides);
        Assert.NotNull(proxies[0].ResponseOverrides);
        Assert.Equal(200, proxies[0].ResponseOverrides!.StatusCode);
        Assert.Equal("OK", proxies[0].ResponseOverrides!.StatusReason);
        Assert.Equal("text/plain", proxies[0].ResponseOverrides!.Headers["Content-Type"]);
    }
}
