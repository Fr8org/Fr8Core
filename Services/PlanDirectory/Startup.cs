using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PlanDirectory.Startup))]

namespace PlanDirectory
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}