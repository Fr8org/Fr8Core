using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalFr8Core.Activities
{
    public class Monitor_Fr8_Events_v1 : ExplicitTerminalActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Monitor_Fr8_Events",
            Label = "Monitor Fr8 Events",
            Version = "1",
            Category = ActivityCategory.Monitors,
            NeedsAuthentication = false,
            MinPaneWidth = 380,
            Tags = Tags.Internal,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Monitor }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private Crate PackCrate_EventSubscriptions()
        {
            var subscriptions = new List<string>
            {
                "RouteActivated",
                "RouteDeactivated",
                "ContainerLaunched",
                "ContainerExecutionComplete",
                "ActionExecuted"
            };

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Fr8Core",
                subscriptions.ToArray()
                );
        }

        public Monitor_Fr8_Events_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Run()
        {
            var curEventReport = Payload.CrateContentsOfType<EventReportCM>().First();
            var standardLoggingCM = curEventReport?.EventPayload.CrateContentsOfType<StandardLoggingCM>().First();
            if (standardLoggingCM != null)
            {
                Payload.Add(Crate.FromContent(curEventReport.EventNames, standardLoggingCM));
            }
            Success();
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            //build a controls crate to render the pane
            var eventSubscription = PackCrate_EventSubscriptions();
            var textBlock = ControlHelper.GenerateTextBlock("Monitor Fr8 Events",
                "This Activity doesn't require any configuration.", "well well-lg");
            var curControlsCrate = PackControlsCrate(textBlock);

           // var planActivatedCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "RouteActivated", "13", AvailabilityType.RunTime);
           // var planDeactivatedCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "RouteDeactivated", "13", AvailabilityType.RunTime);
          //  var containerLaunched = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ContainerLaunched", "13", AvailabilityType.RunTime);
           // var containerExecutionComplete = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ContainerExecutionComplete", "13", AvailabilityType.RunTime);
          //  var actionExecuted = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ActionExecuted", "13", AvailabilityType.RunTime);

            Storage.Add(curControlsCrate);
          //  Storage.Add(planActivatedCrate);
           // Storage.Add(planDeactivatedCrate);
//Storage.Add(containerLaunched);
           // Storage.Add(containerExecutionComplete);
           // Storage.Add(actionExecuted);
            Storage.Add(eventSubscription);

            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}