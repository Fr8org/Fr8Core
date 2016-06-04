using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using terminalUtilities.Excel;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Activities
{
    public class AppBuilder_v1 : BaseTerminalActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "AppBuilder",
            Label = "App Builder",
            Version = "1",
            Category = ActivityCategory.Processors,
            NeedsAuthentication = false,
            MinPaneWidth = 400,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RuntimeCrateLabelPrefix = "Standard Data Table";
        private const string RuntimeFieldCrateLabelPrefix = "Run Time Fields From AppBuilder";
        private const string RunFromSubmitButtonLabel = "RunFromSubmitButton";
        public const string CollectionControlsLabel = "Collection";

        /// <summary>
        /// We don't want false clicked events from submit button
        /// after we read it's state we reset it to unclicked state
        /// </summary>
        /// <param name="curActivityDO"></param>
        private void UnClickSubmitButton()
        {
            var collectionControls = Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).First();
                var submitButton = collectionControls.FindByName<Button>("submit_button");
                submitButton.Clicked = false;
            }

        private async Task PushLaunchURLNotification()
        {
            var msg = "This Plan can be launched with the following URL: " +
                                    CloudConfigurationManager.GetSetting("CoreWebServerUrl") +
                                "redirect/cloneplan?id=" + ActivityId;

            await PushUserNotification("success", "Plan URL", msg);
        }

        private async Task UpdateMetaControls()
        {
            Storage.RemoveByLabel(CollectionControlsLabel);
            var controls = CreateCollectionControlsCrate();
            AddFileDescriptionToStorage(Storage, controls.Get<StandardConfigurationControlsCM>().Controls.Where(a => a.Type == ControlTypes.FilePicker).ToList());
            Storage.Add(controls);

            await HubCommunicator.SaveActivity(ActivityContext.ActivityPayload);
            await PushLaunchURLNotification();
        }

        private StandardConfigurationControlsCM GetMetaControls()
        {
            //user might have pressed submit button on Collection UI
            return Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();
        }

        /// <summary>
        /// TODO this part should be modified with 2975
        /// run logic should be applied here
        /// currently we will only publish textbox fields
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="collectionControls"></param>
        private void PublishCollectionControls()
        {
            var controlContainer = GetControl<MetaControlContainer>("control_container");
            var collectionControls = controlContainer.CreateControls();

            //**** tony
            var userDefinedPayload =
                collectionControls.Select(x => new FieldDTO() {Key = x.Label, Value = x.Label, Availability = AvailabilityType.Always, SourceActivityId = this.ActivityId.ToString()}).ToArray();

            var fieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(RuntimeFieldCrateLabelPrefix, AvailabilityType.RunTime, userDefinedPayload);
            //var fieldsCrate = CrateManager.CreateDesignTimeFieldsCrate(RuntimeFieldCrateLabelPrefix, AvailabilityType.RunTime, new FieldDTO[] { });
            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeFieldCrateLabelPrefix, true).AddFields(userDefinedPayload);
            //**** tony

            Storage.RemoveByLabel(RuntimeFieldCrateLabelPrefix);
            Storage.Add(fieldsCrate);

            
            //tony.y: this not works ↓
            //foreach (var controlDefinitionDTO in collectionControls)
            //    {
            //    PublishCollectionControl(controlDefinitionDTO);
            //    }

                //TODO this part should be modified with 2975
                //PublishFilePickers(pStorage, collectionControls.Controls.Where(a => a.Type == ControlTypes.FilePicker));
            }

        private void PublishCollectionControl(ControlDefinitionDTO controlDefinitionDTO)
        {
            if (controlDefinitionDTO is TextBox)
            {
                PublishTextBox((TextBox)controlDefinitionDTO);
            }
        }

        /*
        public override async Task FollowUp()
        {
            if(ConfigurationControls.Controls[0].Value != null)
            {
                ActivityPayload.Label = ConfigurationControls.Controls[0].Value;
            }

            var controlContainer = ConfigurationControls.FindByName<MetaControlContainer>("control_container");
            if (!controlContainer.MetaDescriptions.Any())
            {
                //TODO add error label
                return;
            }

            //user might have pressed submit button on Collection UI
            var collectionControls = GetMetaControls();
            if (collectionControls != null)
            {
                var submitButton = collectionControls.FindByName<Button>("submit_button");
                if (submitButton.Clicked)
                {
                    if (ActivityPayload.RootPlanNodeId == null)
                    {
                        throw new Exception($"Activity with id \"{ActivityId}\" has no owner plan");
                    }
                    
                    var flagCrate = CrateManager.CreateDesignTimeFieldsCrate(RunFromSubmitButtonLabel,
                        AvailabilityType.RunTime);
                    var payload = new List<CrateDTO>() {CrateManager.ToDto(flagCrate)};
                    //we need to start the process - run current plan - that we belong to
                    await HubCommunicator.RunPlan(ActivityPayload.RootPlanNodeId.Value, payload, CurrentUserId);
                    //after running the plan - let's reset button state
                    //so next configure calls will be made with a fresh state
                    UnClickSubmitButton();
                }
            }
        }
        */
        private bool WasActivityRunFromSubmitButton()
        {
            return Payload.CratesOfType<FieldDescriptionsCM>(c => c.Label == RunFromSubmitButtonLabel).Any();
        }

        private void RemoveFlagCrate()
        {
            Payload.RemoveByLabel(RunFromSubmitButtonLabel);
            }

        private static string GetUriFileExtension(string uri)
        {
            Uri myUri1 = new Uri(uri);
            string path1 = $"{myUri1.Scheme}{Uri.SchemeDelimiter}{myUri1.Authority}{myUri1.AbsolutePath}";
            return Path.GetExtension(path1);
        }

        private async Task<byte[]> ProcessExcelFile(string filePath)
        {
            var byteArray = await new ExcelUtils().GetExcelFileAsByteArray(filePath);
            var payloadCrate = Crate.FromContent(RuntimeCrateLabelPrefix, ExcelUtils.GetExcelFile(byteArray, filePath, false), AvailabilityType.RunTime);
            Payload.Add(payloadCrate);
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

        private void PublishTextBox(TextBox textBox)
        {
            var fieldsCrate = Storage.CratesOfType<FieldDescriptionsCM>(c => c.Label == RuntimeFieldCrateLabelPrefix).First();
            fieldsCrate.Content.Fields.Add(new FieldDTO(textBox.Label, textBox.Label));
        }

        private void ProcessTextBox(TextBox textBox)
        {
            var fieldsCrate = Payload.CratesOfType<StandardPayloadDataCM>(c => c.Label == RuntimeFieldCrateLabelPrefix).First();
            fieldsCrate.Content.PayloadObjects[0].PayloadObject.Add(new FieldDTO(textBox.Label, textBox.Value));
        }

        private async Task ProcessFilePickers( IEnumerable<ControlDefinitionDTO> filepickers)
        {
            int labeless_pickers = 0;
            foreach (FilePicker filepicker in filepickers)
            {
                var uploadFilePath = filepicker.Value;
                byte[] file = null;
                switch (GetUriFileExtension(uploadFilePath))
                {
                    case ".xlsx":
                        file = await ProcessExcelFile(uploadFilePath);
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

                Payload.Add(Crate.FromContent(crate_label, fileDescription, AvailabilityType.RunTime));
            }
        }

        private void ProcessCollectionControl(ControlDefinitionDTO controlDefinitionDTO)
        {
            if (controlDefinitionDTO is TextBox)
            {
                ProcessTextBox((TextBox)controlDefinitionDTO);
            }
        }

        private async Task ProcessCollectionControls(StandardConfigurationControlsCM collectionControls)
        {
            var fieldsPayloadCrate = Crate.FromContent(RuntimeFieldCrateLabelPrefix, new StandardPayloadDataCM(new FieldDTO[] { }), AvailabilityType.RunTime);
            Payload.Add(fieldsPayloadCrate);

            foreach (var controlDefinitionDTO in collectionControls.Controls)
            {
                ProcessCollectionControl(controlDefinitionDTO);
            }

            await ProcessFilePickers(collectionControls.Controls.Where(a => a.Type == ControlTypes.FilePicker));
        }

        protected Crate CreateCollectionControlsCrate()
        {
            var controlContainer = GetControl<MetaControlContainer>("control_container");
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

        public AppBuilder_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        
        public override async Task Run()
        {
            //let's put the file to payload
            //user might have pressed submit button on Collection UI
            var collectionControls = Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();

            //did we run from run button upon PlanBuilder or from submit button inside activity?
            if (collectionControls == null || !WasActivityRunFromSubmitButton())
            {
                //this was triggered by run button on screen
                //not from submit button
                //let's just activate and return
                await UpdateMetaControls();
                //await PushLaunchURLNotification(curActivityDO);
                TerminateHubExecution();
                return;
            }
            RemoveFlagCrate();
            //this means we were run by clicking the submit button
            await ProcessCollectionControls(collectionControls);
            Success();
        }

        public override Task Initialize()
        {
            var configurationControlsCrate = CreateInitialControlsCrate();
            Storage.Add(configurationControlsCrate);
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            if (ConfigurationControls.Controls[0].Value != null)
            {
                ActivityContext.ActivityPayload.Label = ConfigurationControls.Controls[0].Value;
            }
            var controlContainer = GetControl<MetaControlContainer>("control_container");
            if (!controlContainer.MetaDescriptions.Any())
            {
                //TODO add error label
                return;
            }

            //user might have pressed submit button on Collection UI
            var collectionControls = GetMetaControls();
            if (collectionControls != null)
            {

                var submitButton = collectionControls.FindByName<Button>("submit_button");
                if (submitButton.Clicked)
                {
                    if (ActivityContext.ActivityPayload.RootPlanNodeId == null)
                    {
                        throw new Exception($"Activity with id \"{ActivityId}\" has no owner plan");
                    }

                    var flagCrate = CrateManager.CreateDesignTimeFieldsCrate(RunFromSubmitButtonLabel,
                        AvailabilityType.RunTime);
                    var payload = new List<CrateDTO>() { CrateManager.ToDto(flagCrate) };
                    //we need to start the process - run current plan - that we belong to
                    HubCommunicator.RunPlan(ActivityContext.ActivityPayload.RootPlanNodeId.Value, payload);
                    //after running the plan - let's reset button state
                    //so next configure calls will be made with a fresh state
                    UnClickSubmitButton();
                    return;
                }
            }
            PublishCollectionControls();


            // tony.y: i don`t really know what i`m doing here ↓
            //var valuesList = controlContainer.CreateControls() ;

            //foreach (var item in valuesList)
            //{
            //    CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeFieldCrateLabelPrefix, true)
            //        .AddField(item.Label);
            //}

            //var fieldListControl = GetControl<FieldList>("Selected_Fields");

            //var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);

            //userDefinedPayload.ForEach(x =>
            //{
            //    x.Value = x.Key;
            //    x.Availability = AvailabilityType.RunTime;
            //});

            //CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeFieldCrateLabelPrefix, true).AddFields(userDefinedPayload);



           
           
        }
    }
}