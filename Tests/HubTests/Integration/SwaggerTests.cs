using System;
using System.Threading.Tasks;
using Fr8.Testing.Integration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HubTests.Integration
{
    [Explicit]
    [SkipLocal]
    public class SwaggerTests : BaseHubIntegrationTest
    {
        public override string TerminalName => "Hub";

        [Test]
        public async Task Swagger_Always_UpAndRunning()
        {
            var swaggerUrl = $"{GetHubBaseUrl()}swagger/docs/v1";
            var response = await this.RestfulServiceClient.GetAsync(new Uri(swaggerUrl));
            var parsedResponse = JsonConvert.DeserializeObject<JObject>(response);
            JToken swaggerValue;
            if (!parsedResponse.TryGetValue("swagger", out swaggerValue))
            {
                Assert.Fail($"Swagger schema for Hub API is not available at {swaggerUrl}. Received response - {response}");
            }
        }
    }
}
