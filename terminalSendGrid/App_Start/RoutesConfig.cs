using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json.Serialization;

namespace terminalSendGrid
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("SendGrid", config);
        }
    }
}
