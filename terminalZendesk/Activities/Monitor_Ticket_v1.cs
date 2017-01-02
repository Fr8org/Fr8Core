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
using System.Linq;
using Fr8.TerminalBase.Services;
using terminalZendesk.Interfaces;

namespace terminalZendesk.Activities
{
    public class Create_Ticket_v1 : TerminalActivity<Create_Ticket_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("3c8c2408-9f09-415c-b1ce-b69f707b6b12"),
            Name = "Monitor_Ticket",
            Label = "Monitor Ticket",
            Version = "1",
            MinPaneWidth = 330,
            NeedsAuthentication = true,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock Message { get; set; }

            public ActivityUi()
            {
                Message = new TextBlock
                {
                    Label = "Message",
                    Name = nameof(Message),
                    Value = "This activity doesn't need configuration"
                };
                Controls = new List<ControlDefinitionDTO> { Message };
            }
        }

        private const string TicketCreated = "ticketCreated";
        private const string RuntimeCrateLabel = "Monitor Ticket Runtime Fields";

        private const string ZendeskTicketIdField = "Ticket Id";
        private const string ZendeskTicketAssigneeField = "Ticket Assignee";
        private const string ZendeskTicketDescriptionField = "Ticket Description";
        private const string ZendeskTicketTitleField = "Ticket Title";
        private const string ZendeskTicketCreatedTimeField = "Ticket Creation Time";


        public Create_Ticket_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            EventSubscriptions.Manufacturer = "Zendesk";
            EventSubscriptions.Add(TicketCreated);

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel)
                          .AddField(ZendeskTicketIdField)
                          .AddField(ZendeskTicketDescriptionField)
                          .AddField(ZendeskTicketTitleField)
                          .AddField(ZendeskTicketAssigneeField)
                          .AddField(ZendeskTicketCreatedTimeField);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var eventCrate = Payload.CrateContentsOfType<EventReportCM>(x => x.Label == "Zendesk ticket event").FirstOrDefault();
            if (eventCrate == null)
            {
                RequestPlanExecutionTermination("Zendesk event payload was not found");
                return;
            }

            var zendeskEventPayload = eventCrate.EventPayload.CrateContentsOfType<ZendeskTicketCreatedEvent>().FirstOrDefault();
            if (zendeskEventPayload == null)
            {
                RequestPlanExecutionTermination("Zendesk event payload was not found");
                return;
            }

            Payload.Add(Crate<StandardPayloadDataCM>.FromContent(RuntimeCrateLabel, new StandardPayloadDataCM(
                                                                          new KeyValueDTO(ZendeskTicketIdField, zendeskEventPayload.TicketId),
                                                                          new KeyValueDTO(ZendeskTicketDescriptionField, zendeskEventPayload.Description),
                                                                          new KeyValueDTO(ZendeskTicketTitleField, zendeskEventPayload.Title),
                                                                          new KeyValueDTO(ZendeskTicketCreatedTimeField, zendeskEventPayload.Time),
                                                                          new KeyValueDTO(ZendeskTicketAssigneeField, zendeskEventPayload.Assignee))));
        }
    }
}