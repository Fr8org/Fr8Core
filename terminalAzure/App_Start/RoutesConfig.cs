using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using StructureMap;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;
using terminalAzure.Infrastructure;

namespace terminalAzure
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
          BaseTerminalWebApiConfig.Register("Azure",config);
        }
    }
}
