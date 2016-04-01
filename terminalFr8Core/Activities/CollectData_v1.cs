using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Actions
{
    public class CollectData_v1 : BaseTerminalActivity
    {
        private const string RuntimeCrateLabelPrefix = "Standard Data Table";
        private const string RunFromSubmitButtonLabel = "RunFromSubmitButton";
        public const string CollectionControlsLabel = "Collection";
        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateInitialControlsCrate();
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return Task.FromResult(curActivityDO);
        }

        private void UnClickSubmitButton(ActivityDO curActivityDO)
        {
            using (var curStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var collectionControls = curStorage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).First();
                var submitButton = collectionControls.FindByName<Button>("submit_button");
                submitButton.Clicked = false;
            }
        }

        private async Task PushLauncherNotification(ActivityDO curActivityDO)
        {
            await PushUserNotification(new TerminalNotificationDTO
            {
                Type = "Success",
                ActivityName = "CollectData",
                ActivityVersion = "1",
                TerminalName = "terminalFr8Core",
                TerminalVersion = "1",
                Message = "Launcher can be launched with the following URL: " +
                                    CloudConfigurationManager.GetSetting("CoreWebServerUrl") +
                                    "api/v1/planload?id=" + curActivityDO.RootPlanNodeId,
                Subject = "Launcher URL"
            });
        }

        private async Task UpdateMetaControls(ActivityDO curActivityDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var controlContainer = configControls.FindByName<MetaControlContainer>("control_container");
            var storage = CrateManager.GetStorage(curActivityDO);

            using (var curStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var updateButton = GetConfigurationControls(storage).FindByName<Button>("update_launcher");
                if (updateButton.Clicked)
                {
                    updateButton.Clicked = false;
                    curStorage.RemoveByLabel(CollectionControlsLabel);
                    curStorage.Add(CreateCollectionControlsCrate(controlContainer));
                    await PushLauncherNotification(curActivityDO);
                }
            }
        }

        private StandardConfigurationControlsCM GetMetaControls(ActivityDO curActivityDO)
        {
            var storage = CrateManager.GetStorage(curActivityDO);
            //user might have pressed submit button on Collection UI
            return storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var controlContainer = configControls.FindByName<MetaControlContainer>("control_container");
            if (!controlContainer.MetaDescriptions.Any())
            {
                //TODO add error label
                return curActivityDO;
            }

            //user might have pressed submit button on Collection UI
            var collectionControls = GetMetaControls(curActivityDO);
            if (collectionControls != null)
            {
                var submitButton = collectionControls.FindByName<Button>("submit_button");
                if (submitButton.Clicked)
                {
                    if (curActivityDO.RootPlanNodeId == null)
                    {
                        throw new Exception($"Activity with id \"{curActivityDO.Id}\" has no owner plan");
                    }
                    //TODO think of a better way to flag activity
                    var flagCrate = CrateManager.CreateDesignTimeFieldsCrate(RunFromSubmitButtonLabel, AvailabilityType.RunTime);
                    var payload = new List<CrateDTO>(){ CrateManager.ToDto(flagCrate) };
                    //we need to start the process - run current plan - that we belong to
                    await HubCommunicator.RunPlan(curActivityDO.RootPlanNodeId.Value, payload, CurrentFr8UserId);
                    UnClickSubmitButton(curActivityDO);
                }
                else
                {
                    await UpdateMetaControls(curActivityDO);
                }
            }
            else
            {
                await UpdateMetaControls(curActivityDO);
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        private bool WasActivityRunFromSubmitButton(ICrateStorage payloadStorage)
        {
            return payloadStorage.CratesOfType<FieldDescriptionsCM>(c => c.Label == RunFromSubmitButtonLabel).Any();
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);

            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);
            var storage = CrateManager.GetStorage(curActivityDO);

            //let's put the file to payload
            //TODO this activity should be able to put textbox values as StandardPayloadDataCM
            //user might have pressed submit button on Collection UI
            var collectionControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();
            if (collectionControls == null)
            {
                //TODO warn user?
                //no controls were created yet
                return TerminateHubExecution(curPayloadDTO);
            }
            
            //did we run from run button upon PlanBuilder or from submit button inside activity?
            if (!WasActivityRunFromSubmitButton(payloadStorage))
            {
                //this was triggered by run button on screen
                //not from submit button
                //let's just activate and return
                await PushLauncherNotification(curActivityDO);
                return TerminateHubExecution(curPayloadDTO);
            }

            //this means we were run by clicking the submit button

            using (var pStorage = CrateManager.GetUpdatableStorage(curPayloadDTO))
            {
                foreach (var controlDefinitionDTO in collectionControls.Controls)
                {
                    if (controlDefinitionDTO is FilePicker)
                    {
                        var fp = (FilePicker)controlDefinitionDTO;
                        var uploadFilePath = fp.Value;
                        var byteArray = ExcelUtils.GetExcelFileAsByteArray(uploadFilePath);
                        var payloadCrate = Crate.FromContent(RuntimeCrateLabelPrefix, ExcelUtils.GetExcelFile(byteArray, uploadFilePath, false), AvailabilityType.RunTime);
                        pStorage.Add(payloadCrate);

                        //add StandardFileDescriptionCM to payload
                        var fileDescription = new StandardFileDescriptionCM
                        {
                            TextRepresentation = Convert.ToBase64String(byteArray),
                            Filetype = Path.GetExtension(uploadFilePath)
                        };
                        pStorage.Add(Crate.FromContent("File Handler", fileDescription, AvailabilityType.RunTime));
                    }
                }
            }

            return Success(curPayloadDTO);
        }

        protected Crate CreateCollectionControlsCrate(MetaControlContainer controlContainer)
        {
            var generatedConfigControls = controlContainer.CreateControls();
            //let's add a submit button here
            var submitButton = new Button
            {
                CssClass = "float-right mt30 btn btn-default",
                Label = "Submit",
                Name = "submit_button",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            };
            generatedConfigControls.Add(submitButton);
            return Crate<StandardConfigurationControlsCM>.FromContent(CollectionControlsLabel, new StandardConfigurationControlsCM(generatedConfigControls.ToArray()), AvailabilityType.Configuration);
        }

        protected Crate CreateInitialControlsCrate()
        {
            var infoText = new TextBlock()
            {
                Value = "Construct a Launcher that gathers information from users and passes it to another Plan",
                Name = "info_text"
            };

            var cc = new MetaControlContainer()
            {
                Label = "Please insert your desired controls below",
                Name = "control_container"
            };

            var updateButton = new Button
            {
                CssClass = "float-right mt30 btn btn-default",
                Label = "Update Launcher",
                Name = "update_launcher",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            };

            return PackControlsCrate(infoText, cc, updateButton);
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