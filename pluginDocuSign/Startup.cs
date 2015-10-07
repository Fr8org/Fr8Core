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

[assembly: OwinStartup(typeof(terminal_DocuSign.Startup))]

namespace terminal_DocuSign
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
