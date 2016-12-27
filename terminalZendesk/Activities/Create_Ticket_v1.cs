﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;

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

        public const string RuntimeCrateLabel = "Message Built by \"Build Message\" Activity";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            
            public TextBox Name { get; set; }

            public BuildMessageAppender Body { get; set; }

            public ActivityUi()
            {
                Name = new TextBox
                {
                    Label = "Name",
                    Name = nameof(Name),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Body = new BuildMessageAppender
                {
                    Label = "Body",
                    Name = nameof(Body),
                    IsReadOnly = false,
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true,
                        AvailabilityType = AvailabilityType.RunTime
                    },
                    Value = string.Empty
                };
                Controls = new List<ControlDefinitionDTO> { Name, Body };
            }
        }

        public Create_Ticket_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel, true).AddField(ActivityUI.Name.Value);
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            Payload.Add(RuntimeCrateLabel, new StandardPayloadDataCM(new KeyValueDTO(ActivityUI.Name.Value, ActivityUI.Body.Value)));
            return Task.FromResult(0);

        }
    }
}