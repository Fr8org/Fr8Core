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

                ObjectFactory.Initialize();
                ObjectFactory.Configure(Fr8Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);
                ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
                ObjectFactory.Configure(PlanDirectoryBootStrapper.LiveConfiguration);

                ObjectFactory.GetInstance<ISearchProvider>().Initialize(true).Wait();

            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostStartup>(url: url);
        }
    }
}