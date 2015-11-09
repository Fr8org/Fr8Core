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

[assembly: OwinStartup(typeof(terminalSalesforce.Startup))]

namespace terminalSalesforce
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {            
            TerminalSalesforceStructureMapBootstrapper.ConfigureDependencies(TerminalSalesforceStructureMapBootstrapper.DependencyType.LIVE);

            StartHosting("terminal_salesforce");
        }
    }
}
