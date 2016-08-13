using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalAzure
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO WebServiceDTO = new ActivityCategoryDTO
        {
            Name = "Microsoft Azure",
            Type = "WebService"
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