using System.Web;
using Hub.Infrastructure;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PlanDirectory.Startup))]

namespace PlanDirectory
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            OwinInitializer.ConfigureAuth(app, VirtualPathUtility.ToAbsolute("~/Reauthenticate"));
        }
    }
}