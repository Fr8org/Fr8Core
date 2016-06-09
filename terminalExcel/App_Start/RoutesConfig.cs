using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;

namespace terminalExcel
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Excel", config);            
        }
    }
}
