using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using terminalBase;
using terminalBase.BaseClasses;

[assembly: OwinStartup(typeof(pluginDocuSign.Startup))]

namespace pluginDocuSign
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Task.Run(() =>
            {
                BaseTerminalController curController = new BaseTerminalController();
                curController.AfterStartup("plugin_docusign");
            });
        }
    }
}
