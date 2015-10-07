using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;
using terminal_base.BaseClasses;

[assembly: OwinStartup(typeof(terminal_Slack.Startup))]

namespace terminal_Slack
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Task.Run(() =>
            {
                BaseTerminalController curController = new BaseTerminalController();
                curController.AfterStartup("terminal_slack");
            });
        }
    }
}
