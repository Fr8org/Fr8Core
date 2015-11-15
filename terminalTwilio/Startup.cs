using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using terminalTwilio;
using TerminalBase.BaseClasses;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalTwilio
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("terminal_twilio");
        }
    }
}
