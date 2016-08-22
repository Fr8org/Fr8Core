using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;
using Hub.StructureMap;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("DocuSign", config);
        }
    }
}
