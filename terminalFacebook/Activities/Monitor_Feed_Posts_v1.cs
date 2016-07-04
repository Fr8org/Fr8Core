using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using StructureMap;
using terminalFacebook.Interfaces;

namespace terminalFacebook.Activities
{
    public class Monitor_Feed_Posts_v1 : TerminalActivity<Monitor_Feed_Posts_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_Feed_Posts",
            Label = "Monitor Feed Posts",
            Category = ActivityCategory.Monitors,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Categories = new[] { ActivityCategories.Monitor }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string FacebookFeed = "feed";
        private const string RuntimeCrateLabel = "Monitor Facebook Runtime Fields";
        private const string FacebookFeedIdField = "Feed Id";
        private const string FacebookFeedMessageField = "Feed Message";
        private const string FacebookFeedStoryField = "Feed Story";
        private const string FacebookFeedCreatedTimeField = "Feed Time";

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

        private readonly IFacebookIntegration _fbIntegration;

        public Monitor_Feed_Posts_v1(ICrateManager crateManager, IFacebookIntegration fbIntegration)
            : base(crateManager)
        {
            _fbIntegration = fbIntegration;
        }

        private Crate PackEventSubscriptionsCrate()
        {
            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Facebook",
                new string[] { FacebookFeed });
        }

        public override Task Initialize()
        {
            Storage.Add(PackEventSubscriptionsCrate());
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel)
                          .AddField(FacebookFeedIdField)
                          .AddField(FacebookFeedMessageField)
                          .AddField(FacebookFeedStoryField)
                          .AddField(FacebookFeedCreatedTimeField);
            return Task.FromResult(0);
        }


        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            
            return Task.FromResult(0);
        }

        public override Task Activate()
        {
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var eventCrate = Payload.CrateContentsOfType<EventReportCM>(x => x.Label == "Facebook user event").FirstOrDefault();
            if (eventCrate == null)
            {
                TerminateHubExecution("Facebook event payload was not found");
                return;
            }

            var facebookEventPayload = eventCrate.EventPayload.CrateContentsOfType<FacebookUserEventCM>()
                    .FirstOrDefault(e => e.ChangedFields.Contains(FacebookFeed));

            if (facebookEventPayload == null)
            {
                TerminateHubExecution("Facebook event payload was not found");
                return;
            }
            var fbPost = await _fbIntegration.GetPostByTime(AuthorizationToken.Token, facebookEventPayload.Time);

            if (fbPost == null)
            {
                //this probably was a deletion operation
                //let's stop for now
                TerminateHubExecution("Deletions are not handled by monitor feed posts");
                return;
            }

            Payload.Add(Crate<StandardPayloadDataCM>.FromContent(RuntimeCrateLabel, new StandardPayloadDataCM(
                                                                          new KeyValueDTO(FacebookFeedIdField, fbPost.id),
                                                                          new KeyValueDTO(FacebookFeedMessageField, fbPost.message),
                                                                          new KeyValueDTO(FacebookFeedStoryField, fbPost.story),
                                                                          new KeyValueDTO(FacebookFeedCreatedTimeField, fbPost.created_time))));
        }
    }
}