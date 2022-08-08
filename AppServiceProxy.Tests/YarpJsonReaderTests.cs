using AppServiceProxy.Configuration.Yarp;

using Xunit;

namespace AppServiceProxy.Tests;

public class YarpJsonReaderTests
{
    [Fact]
    public void Basic()
    {
        var json = @"
{
  ""Routes"": {
    ""route1"": {
      ""ClusterId"": ""cluster1"",
      ""Match"": {
        ""Path"": ""{**catch-all}""
      }
    }
  },
  ""Clusters"": {
    ""cluster1"": {
      ""Destinations"": {
        ""cluster1/destination1"": {
          ""Address"": ""https://shibayan.jp/""
        }
      }
    }
  }
}
";

        var yarp = YarpJsonReader.ParseJson(json);

        Assert.Equal(1, yarp.Routes.Count);
        Assert.Equal(1, yarp.Clusters.Count);
    }
}
