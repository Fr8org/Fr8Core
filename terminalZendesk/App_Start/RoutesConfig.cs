using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json.Serialization;

namespace terminalZendesk
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Zendesk", config);
        }
    }
}
