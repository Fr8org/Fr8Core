using System;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginAzureSqlServer.Infrastructure;
using StructureMap;

namespace pluginAzureSqlServer
{
    public class PluginAzureSqlServerStructureMapBootstrapper
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
                 For<IPlugin>().Use<Plugin>();
                 For<ICrate>().Use<Crate>();
                 For<IDbProvider>().Use<SqlClientDbProvider>();
                 For<IActivity>().Use<Activity>();
             }
         }
    }
}
