using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using terminalTwilio;
using PluginBase.BaseClasses;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalTwilio
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("plugin_twilio");
        }
    }
}
