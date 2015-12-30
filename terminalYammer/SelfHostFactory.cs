using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using TerminalBase.BaseClasses;

namespace terminalYammer
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();

                WebApiConfig.Register(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
