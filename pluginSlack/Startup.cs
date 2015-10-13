using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using PluginBase;
using PluginBase.BaseClasses;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(pluginSlack.Startup))]

namespace pluginSlack
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            StartHosting("plugin_slack");
        }
    }
}
