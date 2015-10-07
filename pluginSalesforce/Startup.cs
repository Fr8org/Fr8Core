using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;
using terminal_base.BaseClasses;

[assembly: OwinStartup(typeof(terminal_Salesforce.Startup))]

namespace terminal_Salesforce
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            TerminalSalesforceStructureMapBootstrapper.ConfigureDependencies(TerminalSalesforceStructureMapBootstrapper.DependencyType.LIVE);

            Task.Run(() =>
            {
                BaseTerminalController curController = new BaseTerminalController();
                curController.AfterStartup("terminal_salesforce");
            });
        }
    }
}
