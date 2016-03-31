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
using terminalUtilities.Excel;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;
using System.IO;

namespace terminalFr8Core.Actions
{
    public class PlanLauncher_v1 : BaseTerminalActivity
    {
        private const string RuntimeCrateLabelPrefix = "Standard Data Table";
        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return Task.FromResult(curActivityDO);
        }

        private void AddContinueButton(ActivityDO curActivityDO)
        {
            using (var storage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var confControls = GetConfigurationControls(storage);
                var continueButton = new Button
                {
                    CssClass = "float-right mt30 btn btn-default",
                    Label = "Continue",
                    Name = "continue_button",
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onClick", "requestConfig")
                    }
                };
                confControls.Controls.Add(continueButton);
            }
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var crateStorage = CrateManager.GetStorage(curActivityDO);
            var configControls = GetConfigurationControls(crateStorage);

            var continueButton = (Button)configControls.Controls.FirstOrDefault(c => c.Name == "continue_button");
            if (continueButton == null)
            {
                AddContinueButton(curActivityDO);
                return curActivityDO;
            }

            if (!continueButton.Clicked)
            {
                return curActivityDO;
            }

            List<Crate> payloadCrates = new List<Crate>();
            //we are ready to roll
            //let's create a payload
            foreach (var controlDefinitionDTO in configControls.Controls)
            {
                if (controlDefinitionDTO is FilePicker)
                {
                    var fp = (FilePicker)controlDefinitionDTO;
                    var uploadFilePath = fp.Value;
                    var byteArray = ExcelUtils.GetExcelFileAsByteArray(uploadFilePath);
                    var payloadCrate = Crate.FromContent(RuntimeCrateLabelPrefix, ExcelUtils.GetExcelFile(byteArray, uploadFilePath, false), AvailabilityType.RunTime);
                    payloadCrates.Add(payloadCrate);

                    //add StandardFileDescriptionCM to payload
                    var fileDescription = new StandardFileDescriptionCM
                    {
                        TextRepresentation = Convert.ToBase64String(byteArray),
                        Filetype = Path.GetExtension(uploadFilePath)
                    };
                    payloadCrates.Add(Crate.FromContent("File Handler", fileDescription, AvailabilityType.RunTime));
                }
            }

            var crateDTOs = payloadCrates.Select(c => CrateManager.ToDto(c)).ToList();

            var targetPlan = crateStorage.CrateContentsOfType<FieldDescriptionsCM>().Single().Fields.Single();
            try
            {
                await HubCommunicator.RunPlan(Guid.Parse(targetPlan.Value), crateDTOs, CurrentFr8UserId);
                AddInfoText(curActivityDO, "Your request was processed succesfully", false);
            }
            catch
            {
                AddInfoText(curActivityDO, "There has been an error while processing your request", true);
            }

            return curActivityDO;
        }

        private void AddInfoText(ActivityDO curActivityDO, string message, bool isError)
        {
            using (var storage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var confControls = GetConfigurationControls(storage);
                var errorText = (TextBlock)confControls.Controls.FirstOrDefault(c => c.Name == "error_text");

                if (errorText != null)
                {
                    confControls.Controls.Remove(errorText);

                }

                errorText = new TextBlock
                {
                    CssClass = "well well-lg",
                    Name = "error_text",
                    Value = message,
                    ErrorMessage = isError ? message : ""
                };
                confControls.Controls.Add(errorText);
            }
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        private async Task<List<ListItem>> GetUserPlans()
        {
            var plans = await HubCommunicator.GetPlansByName(null, CurrentFr8UserId);
            return plans.Select(x => new ListItem { Key = x.Plan.Name, Value = x.Plan.Id.ToString() }).ToList();
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

            var cc = new MetaControlContainer()
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