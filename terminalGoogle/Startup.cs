using Microsoft.Owin;
using Owin;
using terminalGoogle;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalGoogle
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
