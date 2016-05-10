using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using TerminalBase.Infrastructure;
using System.Threading;
using System.Globalization;
using terminalBox;

namespace terminalDropbox
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // StructureMap Dependencies configuration
            Hub.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            
            TerminalBootstrapper.ConfigureLive();
        }
    }
}