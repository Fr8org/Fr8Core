using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using NUnit.Framework;
using terminalFr8Core.Activities;
using Fr8.Testing.Unit.Fixtures;

namespace Fr8.Testing.Integration.Tools.Activities
{
    public class IntegrationTestTools_terminalFr8
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalFr8(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        /// <summary>
        /// Add Build_Message activity to existing plan and configure the activity with message name and body template
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="ordering"></param>
        /// <param name="messageName"></param>
        /// <param name="messageBodyTemplate"></param>
        /// <returns></returns>
        public async Task<ActivityDTO> AddAndConfigureBuildMessage(PlanDTO plan, int ordering, string messageName, string messageBodyTemplate)
        {
            var activityName = "Build_Message";
            var buildMessageActivityDTO = FixtureData.Build_Message_v1_InitialConfiguration();

            var activityCategoryParam = ActivityCategories.ProcessId.ToString();
            var activityTemplates = await _baseHubITest.HttpGetAsync<List<WebServiceActivitySetDTO>>(_baseHubITest.GetHubApiBaseUrl() + "webservices?id=" + activityCategoryParam);
            var apmActivityTemplate = activityTemplates
                .SelectMany(a => a.Activities)
                .Select(x => new ActivityTemplateSummaryDTO
                {
                    Name = x.Name,
                    Version = x.Version,
                    TerminalName = x.Terminal.Name,
                    TerminalVersion = x.Terminal.Version
                })
                .Single(a => a.Name == activityName);

            buildMessageActivityDTO.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.SubPlans.FirstOrDefault();
            buildMessageActivityDTO.ParentPlanNodeId = subPlan.SubPlanId;
            buildMessageActivityDTO.RootPlanNodeId = plan.Id;
            buildMessageActivityDTO.Ordering = ordering;

            //call initial configuration to server
            buildMessageActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", buildMessageActivityDTO);
            //this call is without authtoken
            buildMessageActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", buildMessageActivityDTO);
            //call followup configuration
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(buildMessageActivityDTO))
            {
                var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                Assert.IsNotNull(controlsCrate, $"{activityName}: Crate StandardConfigurationControlsCM is missing in API response");
                var activityUi = new Build_Message_v1.ActivityUi();
                activityUi.SyncWith(controlsCrate.Content);
                crateStorage.Remove<StandardConfigurationControlsCM>();
                activityUi.Name.Value = messageName;
                activityUi.Body.Value = messageBodyTemplate;
                crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray())));
            }
            buildMessageActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", buildMessageActivityDTO);
            buildMessageActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", buildMessageActivityDTO);

            return buildMessageActivityDTO;
        }

        /// <summary>
        /// Configure Loop_v1 activity. Configure UpstreamCrateChooser to except specific manifestType and crateDescription Label
        /// Assert if upstream crate chooser is configured good based on provided parameters
        /// </summary>
        /// <param name="activityDTO"></param>
        /// <param name="manifestType"></param>
        /// <param name="crateDescriptionLabel"></param>
        /// <returns></returns>
        public async Task<ActivityDTO> ConfigureLoopActivity(ActivityDTO activityDTO, string manifestType, string crateDescriptionLabel)
        {
            activityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", activityDTO);
            activityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", activityDTO);

            using (var loopCrateStorage = _baseHubITest.Crate.GetUpdatableStorage(activityDTO))
            {
                var loopControlsCrate = loopCrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var loopControls = loopControlsCrate.Content.Controls;

                var loopCrateChooser = loopControls.SingleOrDefault(x => x.Type == ControlTypes.CrateChooser && x.Name == "Available_Crates") as CrateChooser;
                // Assert Loop activity has CrateChooser with assigned manifest types.
                Assert.NotNull(loopCrateChooser);
                Assert.AreEqual(1, loopCrateChooser.CrateDescriptions.Count);
                Assert.AreEqual(manifestType, loopCrateChooser.CrateDescriptions[0].ManifestType);
                Assert.IsTrue(loopCrateChooser.CrateDescriptions[0].Label.StartsWith("Spreadsheet Data from"));
            }

            return activityDTO;
        }



        public async Task<ActivityDTO> AddAndConfigureSaveToFr8Warehouse(PlanDTO plan, int ordering)
        {
            var activityName = "Save_To_Fr8_Warehouse";
            var saveToFr8WarehouseActivity = FixtureData.Save_To_Fr8_Warehouse_InitialConfiguration();
            var activityCategoryParam = ActivityCategories.ProcessId.ToString();
            var activityTemplates = await _baseHubITest.HttpGetAsync<List<WebServiceActivitySetDTO>>(_baseHubITest.GetHubApiBaseUrl() + "webservices?id=" + activityCategoryParam);
            var apmActivityTemplate = activityTemplates
                .SelectMany(a => a.Activities)
                .Select(x => new ActivityTemplateSummaryDTO
                {
                    Name = x.Name,
                    Version = x.Version,
                    TerminalName = x.Terminal.Name,
                    TerminalVersion = x.Terminal.Version
                })
                .Single(a => a.Name == activityName);

            saveToFr8WarehouseActivity.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.SubPlans.FirstOrDefault();
            saveToFr8WarehouseActivity.ParentPlanNodeId = subPlan.SubPlanId;
            saveToFr8WarehouseActivity.RootPlanNodeId = plan.Id;
            saveToFr8WarehouseActivity.Ordering = ordering;

            //call initial configuration to server
            saveToFr8WarehouseActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", saveToFr8WarehouseActivity);
            //this call is without authtoken
            saveToFr8WarehouseActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToFr8WarehouseActivity);
            return saveToFr8WarehouseActivity;
        }
    }
}
