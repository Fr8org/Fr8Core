using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;
using terminalInstagram.Interfaces;
using Fr8.Infrastructure.Data.Crates;

namespace terminalInstagram.Actions
{
    public class Monitor_For_New_Media_Posted_v1: TerminalActivity<Monitor_For_New_Media_Posted_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_For_New_Media_Posted",
            Label = "Monitor For New Media Posted v1",
            Category = ActivityCategory.Monitors,
            NeedsAuthentication = true,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private readonly IInstagramIntegration _instagramIntegration;
        private readonly IInstagramEventManager _instagramEventManager;
        public const string ResultPayloadCrateLabel = "Instagram Media";
        public const string EventSubscriptionsCrateLabel = "Standard Event Subscriptions";

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public Monitor_For_New_Media_Posted_v1(ICrateManager crateManager, IInstagramIntegration instagramIntegration)
            : base(crateManager)
        {
            _instagramIntegration = instagramIntegration;
        }

        public override async Task Initialize()
        {
            var oAuthToken = AuthorizationToken.Token;
            Storage.Add(CreateEventSubscriptionCrate());
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(ResultPayloadCrateLabel)
                               .AddFields(GetMediaProperties());
        }
        private IEnumerable<FieldDTO> GetMediaProperties()
        {
            yield return new FieldDTO { Key = "id", Value = "id", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "type", Value = "type", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "object", Value = "object", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "aspect", Value = "aspect", Availability = AvailabilityType.Always };
            yield return new FieldDTO { Key = "callback_url", Value = "callback_url", Availability = AvailabilityType.Always };
        }

        private Crate CreateEventSubscriptionCrate()
        {
            return CrateManager.CreateStandardEventSubscriptionsCrate(EventSubscriptionsCrateLabel, "Slack", "Slack Outgoing Message");
        }
        public override async Task Activate()
        {
            await _instagramEventManager.Subscribe(AuthorizationToken, ActivityPayload.RootPlanNodeId.Value).ConfigureAwait(false);
        }

        public override Task Run()
        {
            throw new NotImplementedException();
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock Description { get; set; }

            public ActivityUi()
            {
                Description = new TextBlock()
                {
                    Value = "This activity will monitor when a new media is shared from your account",
                    Name = "info_text"
                };
                Controls = new List<ControlDefinitionDTO> {Description};
            }
        }
    }
}