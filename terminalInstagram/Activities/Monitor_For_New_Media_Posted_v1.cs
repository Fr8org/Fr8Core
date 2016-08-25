using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalInstagram.Interfaces;
using Fr8.Infrastructure.Data.Crates;
using System.Linq;
using System;

namespace terminalInstagram.Actions
{
    public class Monitor_For_New_Media_Posted_v1: TerminalActivity<Monitor_For_New_Media_Posted_v1.ActivityUi>
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("07FE0129-1B8D-453E-9F80-4E47315A19E8"),
            Name = "Monitor_For_New_Media_Posted",
            Label = "Monitor For New Media Posted",
            NeedsAuthentication = true,
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private readonly IInstagramIntegration _instagramIntegration;
        private readonly IInstagramEventManager _instagramEventManager;

        private const string InstagramMedia = "media";
        private const string RuntimeCrateLabel = "Monitor Instagram Runtime Fields";
        private const string EventSubscriptionsCrateLabel = "Instagram user event";

        private const string InstagramMediaId = "Media Id";
        private const string InstagramCaptionId = "Caption Id";
        private const string InstagramCaptionText = "Caption Text";
        private const string InstagramCaptionCreatedTimeField = "Caption Time";
        private const string InstagramImageUrl = "Image Url";
        private const string InstagramImageUrlStandardResolution = "Image Url Standard Resolution";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock Description { get; set; }

            public ActivityUi()
            {
                Description = new TextBlock()
                {
                    Value = "This activity will monitor when a new media is shared from your account",
                    Label = "Description",
                    Name = nameof(Description)
                };
                Controls = new List<ControlDefinitionDTO> { Description };
            }
        }
        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public Monitor_For_New_Media_Posted_v1(ICrateManager crateManager, IInstagramEventManager instagramEventManager, IInstagramIntegration instagramIntegration)
            : base(crateManager)
        {
            _instagramIntegration = instagramIntegration;
            _instagramEventManager = instagramEventManager;
        }

        public override async Task Initialize()
        {
            EventSubscriptions.Manufacturer = "Instagram";
            EventSubscriptions.Add(InstagramMedia);

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeCrateLabel)
                                            .AddField(InstagramMediaId)
                                            .AddField(InstagramCaptionId)
                                            .AddField(InstagramCaptionText)
                                            .AddField(InstagramCaptionCreatedTimeField)
                                            .AddField(InstagramImageUrl)
                                            .AddField(InstagramImageUrlStandardResolution);
        }
        
        public override async Task Activate()
        {
            await _instagramEventManager.Subscribe().ConfigureAwait(false);
        }

        public override async Task Run()
        {
            var eventCrate = Payload.CrateContentsOfType<EventReportCM>(x => x.Label == "Instagram user event").FirstOrDefault();
            if (eventCrate == null)
            {
                RequestPlanExecutionTermination("Instagram event payload was not found");
                return;
            }

            var instagramEventPayload = eventCrate.EventPayload.CrateContentsOfType<InstagramUserEventCM>()
                    .FirstOrDefault(e => e.ChangedAspect.Contains(InstagramMedia));

            if (instagramEventPayload == null)
            {
                RequestPlanExecutionTermination("Instagram event payload was not found");
                return;
            }
            var instagramPost = await _instagramIntegration.GetPostById(instagramEventPayload.MediaId, AuthorizationToken.Token);

            if (instagramPost == null)
            {
                RequestPlanExecutionTermination("Deletions are not handled by monitor feed posts");
                return;
            }

            Payload.Add(Crate<StandardPayloadDataCM>.FromContent(RuntimeCrateLabel, new StandardPayloadDataCM(
                                                                new KeyValueDTO(InstagramMediaId, instagramPost.data?.id),
                                                                new KeyValueDTO(InstagramCaptionId, instagramPost.data?.caption?.id),
                                                                new KeyValueDTO(InstagramCaptionText, instagramPost.data?.caption?.text),
                                                                new KeyValueDTO(InstagramCaptionCreatedTimeField, instagramPost.data?.caption?.createdTime),
                                                                new KeyValueDTO(InstagramImageUrl, instagramPost.data?.link),
                                                                new KeyValueDTO(InstagramImageUrlStandardResolution, instagramPost.data?.instagramImage.standardResolution.url)
                                                                )));
        }

        
    }
}