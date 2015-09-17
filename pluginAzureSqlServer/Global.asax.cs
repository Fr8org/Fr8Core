using Data.Infrastructure.AutoMapper;
using System;
using System.Web.Http;

namespace pluginAzureSqlServer
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
