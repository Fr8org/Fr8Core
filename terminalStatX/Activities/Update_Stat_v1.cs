using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalStatX.Activities
{
    public class Update_Stat_v1 : EnhancedTerminalActivity<Update_Stat_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Update_Stat",
            Label = "Update Stat",
            Version = "1",
            Category = ActivityCategory.Processors,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
        };

        protected override ActivityTemplateDTO MyTemplate { get; }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox PhoneNumber { get; set; }

            public ActivityUi()
            {
                PhoneNumber = new TextBox()
                    { Name = "PhoneNumber", Label = "Enter the phone number for authentication: " };
                PhoneNumber.Events.Add(new ControlEvent("onChange", "requestConfig"));
                Controls.Add(PhoneNumber);



            }
        }

        public Update_Stat_v1(ICrateManager crateManager) : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            throw new NotImplementedException();
        }

        public override Task FollowUp()
        {
            throw new NotImplementedException();
        }

        public override Task Run()
        {
            throw new NotImplementedException();
        }

    }
}