using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using pluginTwilio;
using PluginBase.BaseClasses;

[assembly: OwinStartup(typeof(Startup))]

namespace pluginTwilio
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Task.Run(() =>
            {
                BasePluginController curController = new BasePluginController();
                curController.AfterStartup(Settings.PluginName);
            });
        }
    }
}
