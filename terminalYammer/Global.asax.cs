using System.Web.Http;
using TerminalBase.Infrastructure;
using System.Globalization;
using System.Threading;

namespace terminalYammer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
<<<<<<< Updated upstream

=======
        protected void Application_Start()
        {
            if (CultureInfo.CurrentCulture.Parent.LCID != 9)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(1033);
            }
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            TerminalBootstrapper.ConfigureLive();
        }
>>>>>>> Stashed changes
    }
}
