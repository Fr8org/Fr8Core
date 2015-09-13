using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CoreActions.Startup))]

namespace CoreActions
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
