using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using terminalUtilities.Excel;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Actions
{
    public class AppBuilder_v1 : BaseTerminalActivity
    {
        private const string RuntimeCrateLabelPrefix = "Standard Data Table";
        private const string RuntimeFieldCrateLabelPrefix = "Run Time Fields From AppBuilder";
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

        /// <summary>
        /// We don't want false clicked events from submit button
        /// after we read it's state we reset it to unclicked state
        /// </summary>
        /// <param name="curActivityDO"></param>
        private void UnClickSubmitButton(ActivityDO curActivityDO)
        {
            using (var curStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var collectionControls = curStorage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).First();
                var submitButton = collectionControls.FindByName<Button>("submit_button");
                submitButton.Clicked = false;
            }
        }

        private async Task PushLaunchURLNotification(ActivityDO curActivityDO)
        {
            await PushUserNotification(new TerminalNotificationDTO
            {
                Type = "Success",
                ActivityName = "AppBuilder",
                ActivityVersion = "1",
                TerminalName = "terminalFr8Core",
                TerminalVersion = "1",
                Message = "This Plan can be launched with the following URL: " +
                                    CloudConfigurationManager.GetSetting("CoreWebServerUrl") +
                                    "redirect/cloneplan?id=" + curActivityDO.RootPlanNodeId,
                                    //"api/v1/plans/clone?id=" + curActivityDO.RootPlanNodeId,
                Subject = "Plan URL"
            });
        }

        private async Task UpdateMetaControls(ActivityDO curActivityDO)
        {
            using (var curStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                curStorage.RemoveByLabel(CollectionControlsLabel);
                var controls = CreateCollectionControlsCrate(curStorage);
                AddFileDescriptionToStorage(curStorage, controls.Get<StandardConfigurationControlsCM>().Controls.Where(a => a.Type == ControlTypes.FilePicker).ToList());
                curStorage.Add(controls);
            }

            await HubCommunicator.SaveActivity(curActivityDO, CurrentFr8UserId);
            await PushLaunchURLNotification(curActivityDO);
        }

        private StandardConfigurationControlsCM GetMetaControls(ActivityDO curActivityDO)
        {
            var storage = CrateManager.GetStorage(curActivityDO);
            //user might have pressed submit button on Collection UI
            return storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();
        }

        /// <summary>
        /// TODO this part should be modified with 2975
        /// run logic should be applied here
        /// currently we will only publish textbox fields
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="collectionControls"></param>
        private void PublishCollectionControls(ActivityDO curActivityDO)
        {
            var configControls = GetConfigurationControls(curActivityDO);
            var controlContainer = configControls.FindByName<MetaControlContainer>("control_container");
            var collectionControls = controlContainer.CreateControls();

            using (var pStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var fieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(RuntimeFieldCrateLabelPrefix, AvailabilityType.RunTime, new FieldDTO[] { });
                pStorage.RemoveByLabel(RuntimeFieldCrateLabelPrefix);
                pStorage.Add(fieldsCrate);

                foreach (var controlDefinitionDTO in collectionControls)
                {
                    PublishCollectionControl(pStorage, controlDefinitionDTO);
                }

                //TODO this part should be modified with 2975
                //PublishFilePickers(pStorage, collectionControls.Controls.Where(a => a.Type == ControlTypes.FilePicker));
            }
        }

        private void PublishCollectionControl(IUpdatableCrateStorage pStorage, ControlDefinitionDTO controlDefinitionDTO)
        {
            if (controlDefinitionDTO is TextBox)
            {
                PublishTextBox(pStorage, (TextBox)controlDefinitionDTO);
            }
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var crateStorage = CrateManager.GetStorage(curActivityDO);
            var configControls = GetConfigurationControls(crateStorage);
            if(configControls.Controls[0].Value != null)
            {
                curActivityDO.Label = configControls.Controls[0].Value;
                return curActivityDO;
            }
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
                    
                    var flagCrate = CrateManager.CreateDesignTimeFieldsCrate(RunFromSubmitButtonLabel,
                        AvailabilityType.RunTime);
                    var payload = new List<CrateDTO>() {CrateManager.ToDto(flagCrate)};
                    //we need to start the process - run current plan - that we belong to
                    await HubCommunicator.RunPlan(curActivityDO.RootPlanNodeId.Value, payload, CurrentFr8UserId);
                    //after running the plan - let's reset button state
                    //so next configure calls will be made with a fresh state
                    UnClickSubmitButton(curActivityDO);
                    return curActivityDO;
                }
            }
            
            PublishCollectionControls(curActivityDO);
            return curActivityDO;
        }

        private bool WasActivityRunFromSubmitButton(ICrateStorage payloadStorage)
        {
            return payloadStorage.CratesOfType<FieldDescriptionsCM>(c => c.Label == RunFromSubmitButtonLabel).Any();
        }

        private void RemoveFlagCrate(PayloadDTO payloadDTO)
        {
            using (var pStorage = CrateManager.GetUpdatableStorage(payloadDTO))
            {
                pStorage.RemoveByLabel(RunFromSubmitButtonLabel);
            }
        }

        private static string GetUriFileExtension(string uri)
        {
            Uri myUri1 = new Uri(uri);
            string path1 = $"{myUri1.Scheme}{Uri.SchemeDelimiter}{myUri1.Authority}{myUri1.AbsolutePath}";
            return Path.GetExtension(path1);
        }

        private byte[] ProcessExcelFile(IUpdatableCrateStorage pStorage, string filePath)
        {
            var byteArray = ExcelUtils.GetExcelFileAsByteArray(filePath);
            var payloadCrate = Crate.FromContent(RuntimeCrateLabelPrefix, ExcelUtils.GetExcelFile(byteArray, filePath, false), AvailabilityType.RunTime);
            pStorage.Add(payloadCrate);
            return byteArray;
        }

        private void AddFileDescriptionToStorage(ICrateStorage storage, List<ControlDefinitionDTO> file_pickers)
        {
            int labelless_filepickers = 0;
            storage.RemoveByManifestId((int)MT.StandardFileHandle);

            foreach (var filepicker in file_pickers)
            {
                string crate_label = GetFileDescriptionLabel(filepicker, labelless_filepickers);
                storage.Add(Crate.FromContent(crate_label, new StandardFileDescriptionCM(), AvailabilityType.RunTime));
            }
        }

        private string GetFileDescriptionLabel(ControlDefinitionDTO filepicker, int labeless_filepickers)
        { return filepicker.Label ?? ("File from App Builder #" + ++labeless_filepickers); }

        private void PublishTextBox(IUpdatableCrateStorage pStorage, TextBox textBox)
        {
            var fieldsCrate = pStorage.CratesOfType<FieldDescriptionsCM>(c => c.Label == RuntimeFieldCrateLabelPrefix).First();
            fieldsCrate.Content.Fields.Add(new FieldDTO(textBox.Label, textBox.Label));
        }

        private void ProcessTextBox(IUpdatableCrateStorage pStorage, TextBox textBox)
        {
            var fieldsCrate = pStorage.CratesOfType<StandardPayloadDataCM>(c => c.Label == RuntimeFieldCrateLabelPrefix).First();
            fieldsCrate.Content.PayloadObjects[0].PayloadObject.Add(new FieldDTO(textBox.Label, textBox.Value));
        }

        private void ProcessFilePickers(IUpdatableCrateStorage pStorage, IEnumerable<ControlDefinitionDTO> filepickers)
        {
            int labeless_pickers = 0;
            foreach (FilePicker filepicker in filepickers)
            {
                var uploadFilePath = filepicker.Value;
                byte[] file = null;
                switch (GetUriFileExtension(uploadFilePath))
                {
                    case ".xlsx":
                        file = ProcessExcelFile(pStorage, uploadFilePath);
                        break;
                }

                if (file == null) continue;

                string crate_label = GetFileDescriptionLabel(filepicker, labeless_pickers);
                var fileDescription = new StandardFileDescriptionCM
                {
                    Filename = Path.GetFileName(uploadFilePath),
                    TextRepresentation = Convert.ToBase64String(file),
                    Filetype = Path.GetExtension(uploadFilePath)
                };

                pStorage.Add(Crate.FromContent(crate_label, fileDescription, AvailabilityType.RunTime));
            }
        }

        private void ProcessCollectionControl(IUpdatableCrateStorage pStorage, ControlDefinitionDTO controlDefinitionDTO)
        {
            if (controlDefinitionDTO is TextBox)
            {
                ProcessTextBox(pStorage, (TextBox)controlDefinitionDTO);
            }
        }

        private void ProcessCollectionControls(PayloadDTO payloadDTO, StandardConfigurationControlsCM collectionControls)
        {
            using (var pStorage = CrateManager.GetUpdatableStorage(payloadDTO))
            {
                var fieldsPayloadCrate = Crate.FromContent(RuntimeFieldCrateLabelPrefix, new StandardPayloadDataCM(new FieldDTO[] { }), AvailabilityType.RunTime);
                pStorage.Add(fieldsPayloadCrate);

                foreach (var controlDefinitionDTO in collectionControls.Controls)
                {
                    ProcessCollectionControl(pStorage, controlDefinitionDTO);
                }

                ProcessFilePickers(pStorage, collectionControls.Controls.Where(a => a.Type == ControlTypes.FilePicker));
            }
        }



        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);

            var payloadStorage = CrateManager.GetStorage(curPayloadDTO);
            var storage = CrateManager.GetStorage(curActivityDO);

            //let's put the file to payload
            //user might have pressed submit button on Collection UI
            var collectionControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();

            //did we run from run button upon PlanBuilder or from submit button inside activity?
            if (collectionControls == null || !WasActivityRunFromSubmitButton(payloadStorage))
            {
                //this was triggered by run button on screen
                //not from submit button
                //let's just activate and return
                await UpdateMetaControls(curActivityDO);
                //await PushLaunchURLNotification(curActivityDO);
                return TerminateHubExecution(curPayloadDTO);
            }

            RemoveFlagCrate(curPayloadDTO);
            //this means we were run by clicking the submit button
            ProcessCollectionControls(curPayloadDTO, collectionControls);

            return Success(curPayloadDTO);
        }

        protected Crate CreateCollectionControlsCrate(ICrateStorage crateStorage)
        {
            var configControls = GetConfigurationControls(crateStorage);
            var controlContainer = configControls.FindByName<MetaControlContainer>("control_container");
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
            var Label = new TextBox
            {
                Label = "App Name",
                Name = "AppLabel",
                Events = new List<ControlEvent> { ControlEvent.RequestConfig }
            };
            var infoText = new TextBlock()
            {
                Value = "Create a form to app builder. Fr8 will generate a URL that you can distribute to users. The URL will launch this plan and collect data.",
                Name = "info_text"
            };

            var cc = new MetaControlContainer()
            {
                Label = "Show which form fields:",
                Name = "control_container",
                Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
            };

            return PackControlsCrate(Label,infoText, cc);
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