using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using fr8.Infrastructure.Data.DataTransferObjects;
using fr8.Infrastructure.Data.States;
using fr8.Infrastructure.Utilities.Configuration;

namespace terminalAzure
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Microsoft Azure"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalAzure.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalAzure",
            Label = "Azure",
            Version = "1"
        };
    }
}