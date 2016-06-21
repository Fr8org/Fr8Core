using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.BaseClasses;
using terminalBasecamp.Infrastructure;

namespace terminalBasecamp.Activities
{
    public class Create_Message_v1 : TerminalActivity<Create_Message_v1.ActivityUi>
    {
        private readonly IBasecampApi _apiClient;

        public static ActivityTemplateDTO ActivityTemplate = new ActivityTemplateDTO
        {
            Name = "Create_Message",
            Label = "Create Message",
            Category = ActivityCategory.Forwarders,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true
        };

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList AccountSelector { get; set; }

            public DropDownList ProjectSelector { get; set; }

            public TextSource MessageHeader { get; set; }

            public TextSource MessageBody { get; set; }

            public ActivityUi()
            {
                AccountSelector = new DropDownList
                {
                    Name = nameof(AccountSelector),
                    Label = "Select Account",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Add(AccountSelector);
                ProjectSelector = new DropDownList
                {
                    Name = nameof(ProjectSelector),
                    Label = "Select Project",
                };
                Controls.Add(ProjectSelector);
                MessageHeader = new TextSource
                {
                    InitialLabel = "Header",
                    Name = nameof(MessageHeader)
                };
                Controls.Add(MessageHeader);
                MessageBody = new TextSource
                {
                    InitialLabel = "Content",
                    Name = nameof(MessageBody)
                };
                Controls.Add(MessageBody);
            }
        }

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplate;

        public Create_Message_v1(ICrateManager crateManager, IBasecampApi apiClient) : base(crateManager)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException(nameof(apiClient));
            }
            _apiClient = apiClient;
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            var projects = await _apiClient.GetAccounts(AuthorizationToken);
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }
    }
}