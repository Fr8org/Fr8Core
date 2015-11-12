using Microsoft.Owin;
using Owin;
using Hub.StructureMap;

[assembly: OwinStartup(typeof(terminalFr8Core.Startup))]

namespace terminalFr8Core
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
