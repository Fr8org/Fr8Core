using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;
using Fr8.TerminalBase.Services;
using terminalZendesk.Interfaces;

namespace terminalZendesk.Activities
{
    public class Create_Ticket_v1 : TerminalActivity<Create_Ticket_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("dfa529ea-1361-4eeb-b0fb-466c17aea73f"),
            Name = "Create_Ticket",
            Label = "Create Ticket",
            Version = "1",
            MinPaneWidth = 330,
            NeedsAuthentication = true,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            
            public TextSource Subject { get; set; }

            public TextSource Body { get; set; }

            public TextSource RequesterEmail { get; set; }

            public TextSource RequesterName { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                Subject = uiBuilder.CreateSpecificOrUpstreamValueChooser("Subject", nameof(Subject), addRequestConfigEvent: true, requestUpstream: true, availability: AvailabilityType.RunTime);
                Body = uiBuilder.CreateSpecificOrUpstreamValueChooser("Body", nameof(Body), addRequestConfigEvent: true, requestUpstream: true, availability: AvailabilityType.RunTime);
                RequesterEmail = uiBuilder.CreateSpecificOrUpstreamValueChooser("RequesterEmail", nameof(RequesterEmail), addRequestConfigEvent: true, requestUpstream: true, availability: AvailabilityType.RunTime);
                RequesterName = uiBuilder.CreateSpecificOrUpstreamValueChooser("RequesterName", nameof(RequesterName), addRequestConfigEvent: true, requestUpstream: true, availability: AvailabilityType.RunTime);
                Controls = new List<ControlDefinitionDTO> { Subject, Body, RequesterEmail, RequesterName };
            }
        }

        private readonly IZendeskIntegration _zendeskIntegration;

        public Create_Ticket_v1(ICrateManager crateManager, IZendeskIntegration zendeskIntegration)
            : base(crateManager)
        {
            _zendeskIntegration = zendeskIntegration;
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var subject = ActivityUI.Subject.TextValue;
            var body = ActivityUI.Body.TextValue;
            var reqMail = ActivityUI.RequesterEmail.TextValue;
            var reqName = ActivityUI.RequesterName.TextValue;
            await _zendeskIntegration.CreateTicket(AuthorizationToken.Token, subject, body, reqMail, reqName);
        }
    }
}