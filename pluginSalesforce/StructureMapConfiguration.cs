using System;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginSalesforce.Infrastructure;
using StructureMap;
using pluginSalesforce.Services;
using Core.StructureMap;
using Data.Infrastructure;
using Data.Interfaces;
using System.Data.Entity;
using Utilities;



namespace pluginSalesforce
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

         public class LiveMode :CoreRegistry
         {
             public LiveMode()
             {
                   For<IDBContext>().Use<DockyardDbContext>();       
             }
         }

         public class CoreRegistry : Registry
         {
             public CoreRegistry()
             {
                 For<IConfigRepository>().Use<ConfigRepository>();
                 For<pluginSalesforce.Infrastructure.IEvent>().Use<pluginSalesforce.Services.Event>();
                 For<IUnitOfWork>().Use(_ => new UnitOfWork(_.GetInstance<IDBContext>()));
                 For<IAction>().Use<Core.Services.Action>();
                 For<ICrate>().Use<Crate>();
                 For<IPlugin>().Use<Plugin>();
                 For<ILead>().Use<pluginSalesforce.Services.Lead>();
                 For<IConfiguration>().Use<pluginSalesforce.Services.Configuration>();
             }
         }
    }
}
