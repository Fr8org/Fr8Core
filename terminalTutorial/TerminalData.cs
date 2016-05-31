using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Configuration.Azure;

namespace terminalTutorial
{
    public static class TerminalData
    {
        public static WebServiceDTO WebServiceDTO = new WebServiceDTO
        {
            Name = "Tutorial"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Name = "terminalTutorial",
            Label = "Tutorial",
            TerminalStatus = TerminalStatus.Active,
            Endpoint = CloudConfigurationManager.GetSetting("terminalTutorial.TerminalEndpoint"),
            Version = "1"
        };
    }
}