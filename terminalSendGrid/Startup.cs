using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;

[assembly: OwinStartup("SendGridStartup", typeof(terminalSendGrid.Startup))]
namespace terminalSendGrid
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("terminal_SendGrid");
        }
    }
}