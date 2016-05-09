using System;
using System.Web.Routing;
using PlanDirectory.App_Start;

namespace PlanDirectory
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs args)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}