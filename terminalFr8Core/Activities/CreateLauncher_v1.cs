using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class CreateLauncher_v1 : BaseTerminalActivity
    {

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = await CreateControlsCrate();
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);

            var generateLauncherButton = configControls.FindByName<Button>("generate_launcher");
            if (!generateLauncherButton.Clicked)
            {
                return curActivityDO;
            }
            
            var launcherName = configControls.FindByName<TextBox>("launcher_name");
            if (string.IsNullOrEmpty(launcherName.Value))
            {
                //TODO add error label
                return curActivityDO;
            }

            var targetPlan = configControls.FindByName<DropDownList>("target_plan");
            if (string.IsNullOrEmpty(targetPlan.Value) || string.IsNullOrEmpty(targetPlan.selectedKey))
            {
                //TODO add error label
                return curActivityDO;
            }

            var controlContainer = configControls.FindByName<ControlContainer>("control_container");
            if (!controlContainer.MetaDescriptions.Any())
            {
                //TODO add error label
                return curActivityDO;
            }


            //we are ready to roll
            var plan = new PlanEmptyDTO
            {
                Name = launcherName.Value,
                PlanState = PlanState.Inactive,
                Description = "Automatically created by CreateLauncher_v1",
                Tag = "auto-created",
                Visibility = PlanVisibility.Standard
            };
            var createdPlan = await HubCommunicator.CreatePlan(plan, CurrentFr8UserId);
            //var startingSubplan = createdPlan.Plan.SubPlans.FirstOrDefault();
            var launchAPlan = await AddAndConfigureChildActivity((Guid)createdPlan.Plan.StartingSubPlanId, "LaunchAPlan", "Launch A Plan", "Launch A Plan", 1);

            using (var storage = CrateManager.GetUpdatableStorage(launchAPlan))
            {
                //let's add newly generated config controls
                var generatedConfigControls = controlContainer.CreateControls();
                storage.Replace(AssembleCrateStorage(PackControlsCrate(generatedConfigControls.ToArray())));
                storage.Add(CrateManager.CreateDesignTimeFieldsCrate("Target Plan", new FieldDTO(targetPlan.selectedKey, targetPlan.Value)));
            }

            launchAPlan = await HubCommunicator.ConfigureActivity(launchAPlan, CurrentFr8UserId);

            //let's add newly created plans url as a textblock
            using (var storage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var currentConfControls = GetConfigurationControls(storage);
                currentConfControls.Controls.RemoveAll(c => c.Name == "url_text");
                currentConfControls.Controls.Add(new TextBlock
                {
                    CssClass = "well",
                    Value = "New plan is generated for this operation, you can check this plan at: "+
#if DEBUG
                    "http://localhost:30643" +
#elif DEV
                    "http://dev.fr8.co" +
#elif RELEASE
                    "http://www.fr8.co" +
#endif
                    "/dashboard#/plans/" + createdPlan.Plan.Id+ "/builder?kioskMode=true",
                    Name = "url_text"
                });
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        private async Task<List<ListItem>> GetUserPlans()
        {
            var plans = await HubCommunicator.GetPlansByName(null, CurrentFr8UserId);
            return plans.Select(x => new ListItem { Key = x.Plan.Name, Value = x.Plan.Id.ToString()}).ToList();
        }

        protected async Task<Crate> CreateControlsCrate()
        {
            var infoText = new TextBlock()
            {
                Value = "Construct a Launcher that gathers information from users and passes it to another Plan",
                Name = "info_text"
            };

            var launcherName = new TextBox()
            {
                Label = "Launcher Name",
                Name = "launcher_name"
            };

            var targetPlan = new DropDownList()
            {
                Name = "target_plan",
                Label = "Target Plan",
                ListItems = await GetUserPlans()
            };

            var cc = new ControlContainer()
            {
                Label = "Please insert your desired controls below",
                Name = "control_container"
            };

            var createButton = new Button
            {
                CssClass = "float-right mt30 btn btn-default",
                Label = "Generate Launcher",
                Name = "generate_launcher",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            };

            return PackControlsCrate(infoText, launcherName, targetPlan, cc, createButton);
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