using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Data.Entities;
using Hub.Managers;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalFr8Core.Actions
{
    public class Monitor_Fr8_Events_v1 : BaseTerminalActivity
    {
        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var eventSubscription = PackCrate_EventSubscriptions();

            var textBlock = GenerateTextBlock("Monitor Fr8 Events",
                "This Activity doesn't require any configuration.", "well well-lg");
            var curControlsCrate = PackControlsCrate(textBlock);

            var planActivatedCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "RouteActivated", "13", AvailabilityType.RunTime);
            var planDeactivatedCrate = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "RouteDeactivated", "13", AvailabilityType.RunTime);
            var containerLaunched = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ContainerLaunched", "13", AvailabilityType.RunTime);
            var containerExecutionComplete = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ContainerExecutionComplete", "13", AvailabilityType.RunTime);
            var actionExecuted = CrateManager.CreateManifestDescriptionCrate("Available Run-Time Objects", "ActionExecuted", "13", AvailabilityType.RunTime);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Add(curControlsCrate);
                crateStorage.Add(planActivatedCrate);
                crateStorage.Add(planDeactivatedCrate);
                crateStorage.Add(containerLaunched);
                crateStorage.Add(containerExecutionComplete);
                crateStorage.Add(actionExecuted);
                crateStorage.Add(eventSubscription);
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var curEventReport = CrateManager.GetStorage(payloadCrates).CrateContentsOfType<EventReportCM>().First();

            if (curEventReport != null)
            {
                var standardLoggingCM = curEventReport.EventPayload.CrateContentsOfType<StandardLoggingCM>().First();

                if (standardLoggingCM != null)
                {
                    using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
                    {
                        crateStorage.Add(Fr8Data.Crates.Crate.FromContent(curEventReport.EventNames, standardLoggingCM));
                    }
                }
            }

            return Success(payloadCrates);
        }

        private Crate PackCrate_EventSubscriptions()
        {
            var subscriptions = new List<string>();
            subscriptions.Add("RouteActivated");
            subscriptions.Add("RouteDeactivated");
            subscriptions.Add("ContainerLaunched");
            subscriptions.Add("ContainerExecutionComplete");
            subscriptions.Add("ActionExecuted");

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Fr8Core",
                subscriptions.ToArray()
                );
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}