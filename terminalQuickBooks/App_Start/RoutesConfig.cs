using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;

namespace terminalQuickBooks
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("QuickBooks", config);
        }
    }
}
