using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Microsoft.Owin;
using Owin;
using Segment;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using Fr8Infrastructure.StructureMap;
using Hub.Infrastructure;
using PlanDirectory.Infrastructure;
using Utilities;

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
