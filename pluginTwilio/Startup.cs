using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using pluginTwilio;
using PluginBase.BaseClasses;

[assembly: OwinStartup(typeof(Startup))]

namespace pluginTwilio
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("plugin_twilio");
        }
    }
}
