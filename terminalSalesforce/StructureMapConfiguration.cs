using System;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using terminalSalesforce.Infrastructure;
using StructureMap;
using terminalSalesforce.Services;
using Core.StructureMap;
using Data.Infrastructure;
using Data.Interfaces;
using Utilities;



namespace terminalSalesforce
{
    public class PluginSalesforceStructureMapBootstrapper
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

         public class LiveMode : StructureMapBootStrapper.LiveMode
         {
             public LiveMode()
             {
                 For<terminalSalesforce.Infrastructure.IEvent>().Use<terminalSalesforce.Services.Event>();
                 For<ISalesforceIntegration>().Use<terminalSalesforce.Services.SalesforceIntegration>();                  
             }
         }       
    }
}
