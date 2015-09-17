using Data.Infrastructure.AutoMapper;
using System.Web.Http;

namespace pluginSlack
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
