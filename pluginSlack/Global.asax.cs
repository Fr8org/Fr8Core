using Data.Infrastructure.AutoMapper;
using System.Web.Http;

namespace terminal_Slack
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
        }
    }
}
