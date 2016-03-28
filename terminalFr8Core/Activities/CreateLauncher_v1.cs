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
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Actions
{
    public class CreateLauncher_v1 : EnhancedTerminalActivity<CreateLauncher_v1.ActivityUi>
    {
        /**********************************************************************************/
        // Configuration controls
        /**********************************************************************************/
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock InfoText;
            public TextBox LauncherName;
            public DropDownList TargetPlan;
            public MetaControlContainer ControlContainer;
            public Button CreateButton;
            public TextBlock UrlInfo;
            

            public ActivityUi()
            {
                InfoText = new TextBlock()
                {
                    Value = "Construct a Launcher that gathers information from users and passes it to another Plan",
                    Name = "info_text"
                };

                LauncherName = new TextBox()
                {
                    Label = "Launcher Name",
                    Name = "launcher_name"
                };

                TargetPlan = new DropDownList()
                {
                    Name = "target_plan",
                    Label = "Target Plan"
                };

                ControlContainer = new MetaControlContainer()
                {
                    Label = "Please insert your desired controls below",
                    Name = "control_container"
                };

                CreateButton = new Button
                {
                    CssClass = "float-right mt30 btn btn-default",
                    Label = "Generate Launcher",
                    Name = "generate_launcher",
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
                };

                UrlInfo = new TextBlock
                {
                    CssClass = "well",
                    Value ="",
                    Name = "url_text",
                    IsHidden = true
                };

                Controls.Add(InfoText);
                Controls.Add(LauncherName);
                Controls.Add(TargetPlan);
                Controls.Add(ControlContainer);
                Controls.Add(CreateButton);
                Controls.Add(UrlInfo);
            }
        }

        public CreateLauncher_v1() : base(false)
        {
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.TargetPlan.ListItems = await GetUserPlans();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            if (!ConfigurationControls.CreateButton.Clicked)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(ConfigurationControls.LauncherName.Value))
            {
                ConfigurationControls.LauncherName.ErrorMessage = "This field must be filled";
                return;
            }

            if (string.IsNullOrEmpty(ConfigurationControls.TargetPlan.Value) || string.IsNullOrEmpty(ConfigurationControls.TargetPlan.selectedKey))
            {
                ConfigurationControls.TargetPlan.ErrorMessage = "This field must be filled";
                return;
            }

            if (!ConfigurationControls.ControlContainer.MetaDescriptions.Any())
            {
                //TODO add error label
                return;
            }


            //we are ready to roll
            var plan = new PlanEmptyDTO
            {
                Name = ConfigurationControls.LauncherName.Value,
                PlanState = PlanState.Inactive,
                Description = "Automatically created by CreateLauncher_v1",
                Tag = "auto-created",
                Visibility = PlanVisibility.Standard
            };
            var createdPlan = await HubCommunicator.CreatePlan(plan, CurrentFr8UserId);
            //var startingSubplan = createdPlan.Plan.SubPlans.FirstOrDefault();
            var launchAPlan = await AddAndConfigureChildActivity((Guid)createdPlan.Plan.StartingSubPlanId, "PlanLauncher", "Plan Launcher", "Plan Launcher", 1);

            using (var storage = CrateManager.GetUpdatableStorage(launchAPlan))
            {
                //let's add newly generated config controls
                var generatedConfigControls = ConfigurationControls.ControlContainer.CreateControls();
                storage.Replace(AssembleCrateStorage(PackControlsCrate(generatedConfigControls.ToArray())));
                storage.Add(CrateManager.CreateDesignTimeFieldsCrate("Target Plan", new FieldDTO(ConfigurationControls.TargetPlan.selectedKey, ConfigurationControls.TargetPlan.Value)));
            }

            launchAPlan = await HubCommunicator.ConfigureActivity(launchAPlan, CurrentFr8UserId);

            ConfigurationControls.UrlInfo.IsHidden = false;
            ConfigurationControls.UrlInfo.Value =
                "Your Launcher Plan has been generated. It is currently visible to you only. Launch this plan by browsing to " +
                CloudConfigurationManager.GetSetting("CoreWebServerUrl") +
                "dashboard#/plans/" + createdPlan.Plan.Id + "/builder?kioskMode=true";
        }

        protected override Task RunCurrentActivity()
        {
            return Task.FromResult(0);
        }

        private async Task<List<ListItem>> GetUserPlans()
        {
            var plans = await HubCommunicator.GetPlansByName(null, CurrentFr8UserId);
            return plans.Select(x => new ListItem { Key = x.Plan.Name, Value = x.Plan.Id.ToString()}).ToList();
        }
    }
}