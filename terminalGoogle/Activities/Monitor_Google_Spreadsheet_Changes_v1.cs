using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using terminalGoogle.Actions;
using terminalGoogle.Interfaces;
using Fr8.Infrastructure.Data.Control;

namespace terminalGoogle.Activities
{
    public class Monitor_Google_Spreadsheet_Changes_v1
        : BaseGoogleTerminalActivity<Monitor_Google_Spreadsheet_Changes_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO()
        {
            Id = new Guid("1DD23540-10A3-4672-AC44-C647C5B235AD"),
            Name = "Monitor_Google_Spreadsheet_Changes",
            Label = "Monitor Google Spreadsheet Changes",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Tags = string.Empty,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.GooogleActivityCategoryDTO
            }
        };

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SpreadsheetList { get; set; }

            public ActivityUi()
            {
                SpreadsheetList = new DropDownList()
                {
                    Label = "Monitor Google Spreadsheet Changes",
                    Name = nameof(SpreadsheetList),
                    Required = true,
                    Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
                };
            }
        }

        public Monitor_Google_Spreadsheet_Changes_v1(
            ICrateManager crateManager,
            IGoogleIntegration googleIntegration)
                : base(crateManager, googleIntegration)
        {
        }

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override async Task FollowUp()
        {
            await Task.Yield();
        }

        public override async Task Initialize()
        {
            await Task.Yield();
        }

        public override async Task Run()
        {
            await Task.Yield();
        }
    }
}