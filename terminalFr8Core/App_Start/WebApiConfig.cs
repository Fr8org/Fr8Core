using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using TerminalBase;
using TerminalBase.BaseClasses;

namespace terminalFr8Core
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Fr8Core", config);
        }
    }
}