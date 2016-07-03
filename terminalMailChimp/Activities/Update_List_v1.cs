using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalMailChimp.Interfaces;

namespace terminalMailChimp.Activities
{
    public class Update_List_v1 : TerminalActivity<Update_List_v1.ActivityUi>
    {
        private readonly IMailChimpIntegration _mailChimpIntegration;

        public Update_List_v1(ICrateManager crateManager, IMailChimpIntegration mailChimpIntegration) : base(crateManager)
        {
            _mailChimpIntegration = mailChimpIntegration;
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList MailChimpListSelector { get; set; }

            public ActivityUi()
            {
                MailChimpListSelector = new DropDownList
                {
                    Label = "Select a MailChimp List",
                    Name = nameof(MailChimpListSelector),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
            }
        }

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Update_List",
            Label = "Update List",
            Version = "1",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override async Task Initialize()
        {
            ActivityUI.MailChimpListSelector.ListItems = (await _mailChimpIntegration.GetLists(AuthorizationToken))
                .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
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