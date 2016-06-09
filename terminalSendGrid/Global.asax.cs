using System.Web;
using Fr8.TerminalBase.Infrastructure;

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
