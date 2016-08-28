using Fr8.Infrastructure.StructureMap;
using Moq;
using StructureMap;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;

namespace terminalSalesforce
{
    public class TerminalSalesforceStructureMapBootstrapper
    {
         public enum DependencyType
         {
             TEST = 0,
             LIVE = 1
         }

         public static void ConfigureDependencies(DependencyType type)
         {
             switch (type)
             {
                 case DependencyType.TEST:
                     ObjectFactory.Configure(x => x.AddRegistry<TestMode>()); 
                     break;
                 case DependencyType.LIVE:
                     ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
                     break;
             }
         }

         public class LiveMode : StructureMapBootStrapper.LiveMode
         {
             public LiveMode()
             {
                 For<IEvent>().Use<Event>();
                 For<ISalesforceManager>().Use<SalesforceManager>();
                 For<ISalesforceFilterBuilder>().Use<SalesforceFilterBuilder>();
             }
         }       

         public class TestMode : StructureMapBootStrapper.TestMode
         {
             public TestMode()
             {
                 For<IEvent>().Use<Event>();

                 var salesforceIntegrationMock = new Mock<ISalesforceManager>(MockBehavior.Default);
                 For<ISalesforceManager>().Use(salesforceIntegrationMock.Object);
                 var salesfirceFilterBuilder = new Mock<ISalesforceFilterBuilder>();
                 For<ISalesforceFilterBuilder>().Use(salesfirceFilterBuilder.Object);
             }
         }       

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
