using System.Net.Http;
using System.Net.Http.Formatting;
using Fr8Infrastructure.Communication;
using Fr8Infrastructure.Interfaces;
using Fr8Infrastructure.Security;
using Moq;
using StructureMap;
using StructureMap.Configuration.DSL;
using ExtternalStructureMap = StructureMap;

namespace Fr8Infrastructure.StructureMap
{
    public class StructureMapBootStrapper
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        public static ExtternalStructureMap.IContainer ConfigureDependencies(DependencyType type)
        {

            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Initialize(x => x.AddRegistry<TestMode>());
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    break;
            }
            return ObjectFactory.Container;
        }

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
                For<IRestfulServiceClient>().Singleton().Use<RestfulServiceClient>().SelectConstructor(() => new RestfulServiceClient());
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