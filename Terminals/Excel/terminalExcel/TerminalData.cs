using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalExcel
{
    public static class TerminalData
    {
        public static ActivityCategoryDTO ActivityCategoryDTO = new ActivityCategoryDTO
        {
            Name = "Excel",
            IconPath = "/Content/icons/web_services/ms-excel-icon-64x64.png",
            Type = "WebService"
        };

        public static TerminalDTO TerminalDTO = new TerminalDTO
        {
            Endpoint = CloudConfigurationManager.GetSetting("terminalExcel.TerminalEndpoint"),
            TerminalStatus = TerminalStatus.Active,
            Name = "terminalExcel",
            Label = "Excel",
            Version = "1"
        };
    }
}