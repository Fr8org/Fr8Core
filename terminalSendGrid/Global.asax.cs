using System.Web;
using TerminalBase.Infrastructure;

namespace terminalSendGrid
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            TerminalSendGridStructureMapBootstrapper.ConfigureDependencies(TerminalSendGridStructureMapBootstrapper.DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
