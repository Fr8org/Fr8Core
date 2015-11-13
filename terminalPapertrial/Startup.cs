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

[assembly: OwinStartup(typeof(terminalPapertrial.Startup))]

namespace terminalPapertrial
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("terminal_papertrial");
        }
    }
}
