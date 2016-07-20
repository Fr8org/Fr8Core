using Fr8.Infrastructure.StructureMap;
using Moq;
using StructureMap;
using terminalSalesforce.Infrastructure;

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
                 For<terminalSalesforce.Infrastructure.IEvent>().Use<terminalSalesforce.Services.Event>();
                 For<ISalesforceManager>().Use<terminalSalesforce.Services.SalesforceManager>();                  
             }
         }       

         public class TestMode : StructureMapBootStrapper.TestMode
         {
             public TestMode()
             {
                 For<terminalSalesforce.Infrastructure.IEvent>().Use<terminalSalesforce.Services.Event>();

                 Mock<ISalesforceManager> salesforceIntegrationMock = new Mock<ISalesforceManager>(MockBehavior.Default);
                 For<ISalesforceManager>().Use(salesforceIntegrationMock.Object);
             }
         }       

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }
    }
}
