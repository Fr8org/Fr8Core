using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using PluginUtilities;

[assembly: OwinStartup(typeof(pluginDocuSign.Startup))]

namespace pluginDocuSign
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            PluginBase.AfterStartup("plugin_docusign");
        }
    }
}
