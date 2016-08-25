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
            Id = new Guid("860b8347-0e5a-41c3-9be7-73057eeca676"),
            Name = "Monitor_Feed_Posts",
            Label = "Monitor Feed Posts",
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
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

        public override Task Initialize()
        {
            EventSubscriptions.Manufacturer = "Facebook";
            EventSubscriptions.Add(FacebookFeed);
            
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
                RequestPlanExecutionTermination("Facebook event payload was not found");
                return;
            }

            var facebookEventPayload = eventCrate.EventPayload.CrateContentsOfType<FacebookUserEventCM>()
                    .FirstOrDefault(e => e.ChangedFields.Contains(FacebookFeed));
            if (facebookEventPayload == null)
            {
                RequestPlanExecutionTermination("Facebook event payload was not found");
                return;
            }
            var fbPost = await _fbIntegration.GetPostById(AuthorizationToken.Token, facebookEventPayload.Id);

            if (fbPost == null)
            {
                //this probably was a deletion operation
                //let's stop for now
                RequestPlanExecutionTermination("Deletions are not handled by monitor feed posts");
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