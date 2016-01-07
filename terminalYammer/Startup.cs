using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(terminalYammer.Startup))]

namespace terminalYammer
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("terminal_Yammer");
        }
    }
}
