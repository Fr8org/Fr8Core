using System.Web;
using Microsoft.Owin;
using Owin;
using Hub.Infrastructure;

[assembly: OwinStartup(typeof(PlanDirectory.Startup))]
namespace PlanDirectory
{
    public class Startup 
    {

        public void Configuration(IAppBuilder app)
        {
            var reauthenticateUrl = VirtualPathUtility.ToAbsolute("~/Reauthenticate");
            OwinInitializer.ConfigureAuth(app, reauthenticateUrl);
        }
    }
}
