using System;
using System.Web.Http;
using Core.Interfaces;
using Core.Services;
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
                For<ICrate>().Use<Crate>();
            }
        }

    }
}
