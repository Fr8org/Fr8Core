//using System;
//using System.Web.Http;
//using Microsoft.Owin.Hosting;
//using Owin;
//using StructureMap.Configuration.DSL;
//using pluginDocuSign.Infrastructure;
//using Core.Interfaces;
//using Core.Services;
//using StructureMap;
//using Data.Infrastructure.StructureMap;
//using Data.Interfaces;
//using DocuSign.Integrations.Client;
//using Data.Wrappers;

//namespace pluginDocuSign
//{
//    public class PluginDocuSignMapBootstrapper
//    {
//        public enum DependencyType
//        {
//            TEST = 0,
//            LIVE = 1
//        }

//        public static void ConfigureDependencies(DependencyType type)
//        {
//            switch (type)
//            {
//                case DependencyType.TEST:
//                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>()); // No test mode yet
//                    break;
//                case DependencyType.LIVE:
//                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
//                    break;
//            }
//        }

//        public class LiveMode : DatabaseStructureMapBootStrapper.LiveMode
//        {
//            public LiveMode()
//            {
//                For<ICrate>().Use<Crate>();
//                For<IAction>().Use<Core.Services.Action>();
//                For<IEnvelope>().Use<pluginDocuSign.Infrastructure.DocuSignEnvelope>();
//                For<IPlugin>().Use<Plugin>();
//                For<IDocuSignTemplate>().Use<DocuSignTemplate>();
//            }
//        }

//        public class CoreRegistry : Registry {
//            public CoreRegistry() {
//            }
//        }
//    }
//}
