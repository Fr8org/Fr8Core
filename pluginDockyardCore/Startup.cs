using Microsoft.Owin;
using Owin;

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
