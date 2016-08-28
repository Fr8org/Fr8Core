using Fr8.TerminalBase.Interfaces;
using Fr8.Testing.Integration;
using StructureMap;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;

namespace terminalSalesforceTests.Intergration
{
    public abstract class BaseSalesforceIntegrationTest : BaseHubIntegrationTest
    {
        protected readonly IContainer _container;

        public BaseSalesforceIntegrationTest()
        {
            _container = ObjectFactory.Container.CreateChildContainer();
            _container.Configure(MockedHubCommunicatorConfiguration);
            _container.Configure(x => x.For<ISalesforceFilterBuilder>().Use<SalesforceFilterBuilder>());
        }

        public static void MockedHubCommunicatorConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<MockedHubCommunicatorRegistry>();
        }

        public class MockedHubCommunicatorRegistry : StructureMap.Configuration.DSL.Registry
        {
            public MockedHubCommunicatorRegistry()
            {
                For<IHubCommunicator>().Use(new Moq.Mock<IHubCommunicator>(Moq.MockBehavior.Default).Object);
            }
        }
    }
}
