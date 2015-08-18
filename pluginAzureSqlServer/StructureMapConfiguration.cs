using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginAzureSqlServer.Infrastructure;

namespace pluginAzureSqlServer
{
    public class PluginAzureSqlServerStructureMapBootstrapper
    {
        public class CoreRegistry : Registry {
            public CoreRegistry() {
                For<IDbProvider>().Use<SqlClientDbProvider>();
            }
        }

    }
}
