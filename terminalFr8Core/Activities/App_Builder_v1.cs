using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalUtilities.Excel;

namespace terminalFr8Core.Activities
{
    public class App_Builder_v1 : ExplicitTerminalActivity
    {
        private readonly ExcelUtils _excelUtils;
        private readonly IPushNotificationService _pushNotificationService;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("04390199-7cfd-4217-bf40-7671e130dc28"),
            Name = "App_Builder",
            Label = "App Builder",
            Version = "1",
            Category = ActivityCategory.Processors,
            NeedsAuthentication = false,
            MinPaneWidth = 320,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        public const string CollectionControlsLabel = "Collection";
        private const string RuntimeCrateLabelPrefix = "Standard Data Table";
        private const string RuntimeFieldCrateLabelPrefix = "Run Time Fields From AppBuilder";
        private const string RunFromSubmitButtonLabel = "RunFromSubmitButton";

        /// <summary>
        /// We don't want false clicked events from submit button after we read it's state we reset it to unclicked state
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
            var msg = "This Plan can be launched with the following URL: " + CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "redirect/cloneplan?id=" + ActivityId;
            await _pushNotificationService.PushUserNotification(MyTemplate, "Success", "App Builder URL Generated", msg);
        }

        private async Task UpdateMetaControls()
        {
            Storage.RemoveByLabel(CollectionControlsLabel);
            var controls = CreateCollectionControlsCrate();
            AddFileDescriptionToStorage(Storage, controls.Get<StandardConfigurationControlsCM>().Controls.Where(a => a.Type == ControlTypes.FilePicker).ToList());
            Storage.Add(controls);

            await HubCommunicator.SaveActivity(ActivityContext.ActivityPayload, true);
            await PushLaunchURLNotification();
        }

        private StandardConfigurationControlsCM GetMetaControls()
        {
            //user might have pressed submit button on Collection UI
            return Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();
        }

        /// <summary>
        /// TODO this part should be modified with 2975
        /// Run logic should be applied here currently we will only publish textbox fields
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="collectionControls"></param>
        private void PublishCollectionControls()
        {
            var controlContainer = GetControl<MetaControlContainer>("control_container");
            var collectionControls = controlContainer.CreateControls();
            //we need our own crate signaller to publish fields without source activity id
            //because when we are cloning the plan for appBuilder ids change
            var configurator = CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RuntimeFieldCrateLabelPrefix, true);

            foreach (var controlDefinitionDTO in collectionControls)
            {
                PublishCollectionControl(controlDefinitionDTO, configurator);
            }

            //TODO this part should be modified with 2975
            //PublishFilePickers(pStorage, collectionControls.Controls.Where(a => a.Type == ControlTypes.FilePicker));
        }

        private void PublishCollectionControl(ControlDefinitionDTO controlDefinitionDTO, CrateSignaller.FieldConfigurator fieldConfigurator)
        {
            if (controlDefinitionDTO is TextBox)
            {
                PublishTextBox((TextBox)controlDefinitionDTO, fieldConfigurator);
            }
        }

        private bool WasActivityRunFromSubmitButton()
        {
            return Payload.CratesOfType<KeyValueListCM>(c => c.Label == RunFromSubmitButtonLabel).Any();
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
            var byteArray = await _excelUtils.GetExcelFileAsByteArray(filePath);
            var payloadCrate = Crate.FromContent(RuntimeCrateLabelPrefix, _excelUtils.GetExcelFile(byteArray, filePath, false), AvailabilityType.RunTime);
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
        {
            return filepicker.Label ?? ("File from App Builder #" + ++labeless_filepickers);
        }

        private void PublishTextBox(TextBox textBox, CrateSignaller.FieldConfigurator fieldConfigurator)
        {
            fieldConfigurator.AddField(textBox.Label);
        }

        private void ProcessTextBox(TextBox textBox)
        {
            var fieldsCrate = Payload.CratesOfType<StandardPayloadDataCM>(c => c.Label == RuntimeFieldCrateLabelPrefix).First();
            fieldsCrate.Content.PayloadObjects[0].PayloadObject.Add(new KeyValueDTO(textBox.Label, textBox.Value));
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
            var fieldsPayloadCrate = Crate.FromContent(RuntimeFieldCrateLabelPrefix, new StandardPayloadDataCM(new KeyValueDTO[] { }));
            fieldsPayloadCrate.SourceActivityId = ActivityId.ToString();

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
            // Let's add a submit button here
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
        protected void CreateInitialControls()
        {
            var label = new TextBox
            {
                Label = "App Name",
                Name = "AppLabel",
                Events = new List<ControlEvent> { ControlEvent.RequestConfig }
            };

            var infoText = new TextBlock()
            {
                Value = "This activity, when run, creates an app that you can distribute to other users as a URL. <a href='http://documentation.fr8.co/action-development-building-documentation/' target='_blank'>?</a>",
                Name = "info_text"
            };

            var cc = new MetaControlContainer()
            {
                Label = "Show which form fields:",
                Name = "control_container",
                Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
            };

            AddControls(label, infoText, cc);
        }

        public App_Builder_v1(ICrateManager crateManager, ExcelUtils excelUtils, IPushNotificationService pushNotificationService)
            : base(crateManager)
        {
            _excelUtils = excelUtils;
            _pushNotificationService = pushNotificationService;
        }


        public override async Task Run()
        {
            //let's put the file to payload
            //user might have pressed submit button on Collection UI
            var collectionControls = Storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => c.Label == CollectionControlsLabel).FirstOrDefault();

            //did we run from run button upon PlanBuilder or from submit button inside activity?
            if (collectionControls == null || !WasActivityRunFromSubmitButton())
            {
                //this was triggered by run button on screen not from submit button. Let's just activate and return
                await UpdateMetaControls();
                //await PushLaunchURLNotification(curActivityDO);
                RequestPlanExecutionTermination();
                return;
            }
            RemoveFlagCrate();
            //this means we were run by clicking the submit button
            await ProcessCollectionControls(collectionControls);
            Success();
        }

        public override Task Initialize()
        {
            CreateInitialControls();
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

                    var flagCrate = CrateManager.CreateDesignTimeFieldsCrate(RunFromSubmitButtonLabel);
                    var payload = new List<CrateDTO>() { CrateManager.ToDto(flagCrate) };

                    
                    await HubCommunicator.SaveActivity(ActivityContext.ActivityPayload);
                    HubCommunicator.RunPlan(ActivityContext.ActivityPayload.RootPlanNodeId.Value, payload);


                    // We must save ourselves before running activity
                    /*
                    HubCommunicator.SaveActivity(ActivityContext.ActivityPayload).ConfigureAwait(false);
                    HubCommunicator.RunPlan(ActivityContext.ActivityPayload.RootPlanNodeId.Value, payload);
                    */


                    //we need to start the process - run current plan - that we belong to
                    //after running the plan - let's reset button state
                    //so next configure calls will be made with a fresh state
                    UnClickSubmitButton();
                    return;
                }
            }
            PublishCollectionControls();
        }
    }
}