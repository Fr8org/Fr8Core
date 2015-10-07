using Microsoft.Owin;
using Owin;
using Core.StructureMap;

[assembly: OwinStartup(typeof(terminal_fr8Core.Startup))]

namespace terminal_fr8Core
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
