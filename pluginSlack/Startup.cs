using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using terminalBase;
using terminalBase.BaseClasses;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(pluginSlack.Startup))]

namespace pluginSlack
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Task.Run(() =>
            {
                BaseTerminalController curController = new BaseTerminalController();
                curController.AfterStartup("plugin_slack");
            });
        }
    }
}
