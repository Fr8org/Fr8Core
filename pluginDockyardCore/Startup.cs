using Microsoft.Owin;
using Owin;
using Core.StructureMap;

[assembly: OwinStartup(typeof(pluginDockyardCore.Startup))]

namespace pluginDockyardCore
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
