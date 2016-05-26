using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Utilities.Configuration.Azure;

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