using Core.Interfaces;
using Core.Services;
using StructureMap;
using StructureMap.Configuration.DSL;
using terminal_Salesforce.Infrastructure;

namespace terminal_Salesforce
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
                     ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>()); // No test mode yet
                     break;
                 case DependencyType.LIVE:
                     ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                     break;
             }
         }

         public class LiveMode : Registry
         {
             public LiveMode()
             {
                 For<IAction>().Use<Core.Services.Action>();
                 For<ICrate>().Use<Crate>();
                 For<IPlugin>().Use<Plugin>();
                 For<ILead>().Use<terminal_Salesforce.Services.Lead>();
                 For<IConfiguration>().Use<terminal_Salesforce.Services.Configuration>();                
             }
         }
    }
}
