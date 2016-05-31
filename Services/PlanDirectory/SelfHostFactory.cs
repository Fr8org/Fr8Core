using System;
using System.Web.Http;
using Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.DataProtection;
using StructureMap;
using Hub.Infrastructure;
using PlanDirectory.App_Start;
using PlanDirectory.Infrastructure;

namespace PlanDirectory
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var configuration = new HttpConfiguration();

                WebApiConfig.Register(configuration);
                app.SetDataProtectionProvider(new DpapiDataProtectionProvider());

                OwinInitializer.ConfigureAuth(app, "/Reauthenticate");
                app.UseWebApi(configuration);

                // if (!selfHost)
                // {
                //     RouteConfig.RegisterRoutes(RouteTable.Routes);
                // 
                //     var segmentWriteKey = new ConfigRepository().Get("SegmentWriteKey");
                //     Analytics.Initialize(segmentWriteKey);
                // }
                // 
                ObjectFactory.Initialize();
                ObjectFactory.Configure(Fr8Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);
                ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
                ObjectFactory.Configure(PlanDirectoryBootStrapper.LiveConfiguration);
                // 
                // DataAutoMapperBootStrapper.ConfigureAutoMapper();
                // 
                // Utilities.Server.ServerPhysicalPath = selfHost ? "/" : HttpContext.Current.Server.MapPath("~");
                // 
                // ObjectFactory.GetInstance<IPlanTemplate>().Initialize().Wait();

            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostStartup>(url: url);
        }
    }
}