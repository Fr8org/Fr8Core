using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using StructureMap.Configuration.DSL;
using pluginDocuSign.Infrastructure;

namespace pluginDocuSign
{
    public class PluginDocuSignMapBootstrapper
    {
        public class CoreRegistry : Registry {
            public CoreRegistry() {
            }
        }
    }
}
