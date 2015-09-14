using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginSlack.Infrastructure;

namespace pluginAzureSqlServer
{
    public class PluginSlackStructureMapBootstrapper
    {
        public class CoreRegistry : Registry {
            public CoreRegistry() {
                For<IDbProvider>().Use<SqlClientDbProvider>();
            }
        }

    }
}
