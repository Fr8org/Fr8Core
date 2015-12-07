using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("DocuSign", config);
        }
    }
}
