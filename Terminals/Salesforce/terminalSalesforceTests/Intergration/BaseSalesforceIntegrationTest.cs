using Fr8.TerminalBase.Interfaces;
using Fr8.Testing.Integration;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalSalesforceTests.Intergration
{
    public abstract class BaseSalesforceIntegrationTest : BaseHubIntegrationTest
    {
        protected readonly IContainer _container;

        public BaseSalesforceIntegrationTest()
        {

            _container = ObjectFactory.Container.CreateChildContainer();
            _container.Configure(MockedHubCommunicatorConfiguration);
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
