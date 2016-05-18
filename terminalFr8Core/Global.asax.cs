using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using System.Web.SessionState;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase.Infrastructure;

namespace terminalFr8Core
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            Fr8CoreStructureMapConfiguration.ConfigureDependencies(Fr8CoreStructureMapConfiguration.DependencyType.LIVE);

            TerminalBootstrapper.ConfigureLive();
        }
    }
}