using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalTest
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Fr8Core", config);
        }
    }
}