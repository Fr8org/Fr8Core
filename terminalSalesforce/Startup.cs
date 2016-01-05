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
using terminalSalesforce;
using TerminalBase.Infrastructure;

[assembly: OwinStartup(typeof(terminalSalesforce.Startup))]

namespace terminalSalesforce
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {            
            TerminalSalesforceStructureMapBootstrapper.ConfigureDependencies(TerminalSalesforceStructureMapBootstrapper.DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
            StartHosting("terminal_Salesforce");
        }
    }
}
