using System.Net.Http;
using System.Net.Http.Formatting;
using Infrastructure.Communication;
using Infrastructure.Interfaces;
using Infrastructure.Security;
using Moq;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Infrastructure.StructureMap
{
    public class StructureMapBootStrapper
    {
        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

        public static void TestConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

        public class LiveMode : Registry
        {
            public LiveMode()
            {
                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();
                For<IHMACAuthenticator>().Use<HMACAuthenticator>();
                For<IHMACService>().Use<Fr8HMACService>();
            }
        }

        public class TestMode : Registry
        {
            public TestMode()
            {
                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();

                var restfulServiceClientMock = new Mock<RestfulServiceClient>(MockBehavior.Default);
                For<IRestfulServiceClient>().Use(restfulServiceClientMock.Object).Singleton();

                var fr8HMACAuthenticator = new Mock<IHMACAuthenticator>();
                fr8HMACAuthenticator.Setup(x => x.IsValidRequest(It.IsAny<HttpRequestMessage>(), It.IsAny<string>())).ReturnsAsync(true);
                var outTerminalId = "testTerminal";
                var outUserId = "testUser";
                fr8HMACAuthenticator.Setup(s => s.ExtractTokenParts(It.IsAny<HttpRequestMessage>(), out outTerminalId, out outUserId));
                For<IHMACAuthenticator>().Use(fr8HMACAuthenticator.Object);

                var fr8HMACService = new Mock<IHMACService>();
                For<IHMACService>().Use(fr8HMACService.Object);
            }
        }
    }
}