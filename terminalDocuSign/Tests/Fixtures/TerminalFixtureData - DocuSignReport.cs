using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace terminalDocuSign.Tests.Fixtures
{
    public partial class TerminalFixtureData
    {
        public static ActivityTemplateDTO Generate_DocuSign_ReportTemplate()
        {
            return new ActivityTemplateDTO
            {
                Name = "Generate_DocuSign_Report",
                Label = "Generate a DocuSign Report",
                Version = "1",
                NeedsAuthentication = true,
                MinPaneWidth = 550,
                Tags = "HideChildren",
                Id = Guid.NewGuid()
            };
        }
    }
}