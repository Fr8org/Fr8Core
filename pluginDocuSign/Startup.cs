using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using terminal_base;
using terminal_base.BaseClasses;

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
                curController.AfterStartup("terminal_docusign");
            });
        }
    }
}
