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
            Id = new System.Guid("e75112ed-e17d-4b90-a337-50a5d59b1866"),
            Name = "Monitor_Fr8_Events",
            Label = "Monitor Fr8 Events",
            Version = "1",
            NeedsAuthentication = false,
            MinPaneWidth = 380,
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        
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
            var textBlock = UiBuilder.GenerateTextBlock("Monitor Fr8 Events",
                "This Activity doesn't require any configuration.", "well well-lg");
            AddControls(textBlock);

            // var planActivatedCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "RouteActivated", "13", AvailabilityType.RunTime);
            // var planDeactivatedCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "RouteDeactivated", "13", AvailabilityType.RunTime);
            //  var containerLaunched = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ContainerLaunched", "13", AvailabilityType.RunTime);
            // var containerExecutionComplete = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ContainerExecutionComplete", "13", AvailabilityType.RunTime);
            //  var actionExecuted = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ActionExecuted", "13", AvailabilityType.RunTime);

            //  Storage.Add(planActivatedCrate);
            // Storage.Add(planDeactivatedCrate);
            //Storage.Add(containerLaunched);
            // Storage.Add(containerExecutionComplete);
            // Storage.Add(actionExecuted);

            EventSubscriptions.Manufacturer = "Fr8Core";
            EventSubscriptions.AddRange("RouteActivated", "RouteDeactivated", "ContainerLaunched", "ContainerExecutionComplete", "ActionExecuted");

            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}