using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Manifests.Helpers;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using Google.GData.Client;
using Newtonsoft.Json;
using terminalGoogle.Actions;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using Fr8.TerminalBase.Services;
using System.Globalization;
using System.Text;
using Fr8.Infrastructure.Data.Constants;
using System.IO;

namespace terminalGoogle.Activities
{
    public class Create_Google_Doc_v1 : BaseGoogleTerminalActivity<Create_Google_Doc_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("DAFD6823-CF54-4DD4-A618-D89A967EB885"),
            Name = "Create_Google_Doc",
            Label = "Create Google Doc",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.GooogleActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser UpstreamCrateChooser { get; set; }

            public DropDownList ExistingFilesList { get; set; }

            public RadioButtonOption UseIncomingDataOption { get; set; }

            public RadioButtonOption UseStoredFileOption { get; set; }

            public RadioButtonGroup ContentSelectionGroup { get; set; }

            public TextSource NewFileName { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                NewFileName = uiBuilder.CreateSpecificOrUpstreamValueChooser("Title", nameof(NewFileName), addRequestConfigEvent: true, requestUpstream: true, availability: AvailabilityType.RunTime);
                NewFileName.IsCollapsed = false;
                Controls.Add(NewFileName);

                UpstreamCrateChooser = new CrateChooser
                {
                    Label = "Crate to store",
                    Name = nameof(UpstreamCrateChooser),
                    Required = true,
                    RequestUpstream = true,
                    SingleManifestOnly = true,
                };

                ExistingFilesList = new DropDownList
                {
                    Name = nameof(ExistingFilesList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                UseIncomingDataOption = new RadioButtonOption
                {
                    Selected = true,
                    Name = nameof(UseIncomingDataOption),
                    Value = "Use Incoming Data",
                    Controls = new List<ControlDefinitionDTO> { UpstreamCrateChooser }
                };

                UseStoredFileOption = new RadioButtonOption()
                {
                    Selected = false,
                    Name = nameof(UseStoredFileOption),
                    Value = "Use Excisting File",
                    Controls = new List<ControlDefinitionDTO> { ExistingFilesList }
                };

                ContentSelectionGroup = new RadioButtonGroup
                {
                    GroupName = nameof(ContentSelectionGroup),
                    Name = nameof(ContentSelectionGroup),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Radios = new List<RadioButtonOption>
                                                         {
                                                             UseIncomingDataOption,
                                                             UseStoredFileOption
                                                         }
                };

                Controls.Add(ContentSelectionGroup);
            }
        }

        private const string SelectedDocCrateLabel = "Selected Doc";

        private readonly IGoogleDrive _googleDrive;

        public Create_Google_Doc_v1(ICrateManager crateManager, IGoogleIntegration googleIntegration, IGoogleDrive googleDrive)
            : base(crateManager, googleIntegration)
        {
            _googleDrive = googleDrive;
        }

        private GoogleAuthDTO GetGoogleAuthToken()
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>(AuthorizationToken.Token);
        }

        public override async Task Initialize()
        {
            ActivityUI.ExistingFilesList.ListItems = await GetCurrentUsersFiles();
        }

        public override async Task FollowUp()
        {
        }

        protected override Task Validate()
        {
            ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.NewFileName, "Title for Document must be specified");

            if(ActivityUI.UseIncomingDataOption.Selected == true)
            {
                ValidationManager.ValidateCrateChooserNotEmpty(ActivityUI.UpstreamCrateChooser, "File must be specified");
            }

            if(ActivityUI.UseStoredFileOption.Selected == true)
            {
                ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.ExistingFilesList, "File must be specified");
            }

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            byte[] body = null;
            string fileName = null;
           
            if(ActivityUI.UseIncomingDataOption.Selected == true)
            {
                var crateToProcess = FindCrateToProcess();
                if (crateToProcess == null)
                {
                    throw new ActivityExecutionException($"Failed to run {ActivityPayload.Name} because specified upstream crate was not found in payload");
                }

                var content = crateToProcess.Get<StandardFileDescriptionCM>();
                body = Convert.FromBase64String(content.TextRepresentation);
                fileName = content.Filename;
            }
            else if(ActivityUI.UseStoredFileOption.Selected == true)
            {
                var fileSelector = ActivityUI.ExistingFilesList;
                if (string.IsNullOrEmpty(fileSelector.Value))
                {
                    RaiseError("No File was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                    return;
                }
                //let's download this file
                var file = await HubCommunicator.DownloadFile(int.Parse(fileSelector.Value));
                if (file == null || file.Length < 1)
                {
                    RaiseError("Unable to download file from Hub");
                    return;
                }
                fileName = fileSelector.selectedKey;
                
                using (var reader = new MemoryStream())
                {
                    file.CopyTo(reader);
                    body = reader.ToArray();
                }                
            }

            try
            {
                var file = await _googleDrive.CreateFile(ActivityUI.NewFileName.TextValue,
                    body, GetMimeType(fileName), GetGoogleAuthToken());

            }
            catch (GDataRequestException ex)
            {
                if (ex.InnerException.Message.IndexOf("(401) Unauthorized") > -1)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException();
                }

                throw;
            }
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        private async Task<List<ListItem>> GetCurrentUsersFiles()
        {
            //Leave only XLSX files as activity fails to rewrite XLS files
            var curAccountFileList = (await HubCommunicator.GetFiles()).Where(x => (x.OriginalFileName?.EndsWith(".doc", StringComparison.InvariantCultureIgnoreCase) ?? true)|| (x.OriginalFileName?.EndsWith(".docx", StringComparison.InvariantCultureIgnoreCase) ?? true));
            //TODO where tags == Docusign files
            return curAccountFileList.Select(c => new ListItem() { Key = c.OriginalFileName, Value = c.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
        }

        private Crate FindCrateToProcess()
        {
            var desiredCrateDescription = ActivityUI.UpstreamCrateChooser.CrateDescriptions.Single(x => x.Selected);
            return Payload.FirstOrDefault(x => x.Label == desiredCrateDescription.Label && x.ManifestType.Type == desiredCrateDescription.ManifestType);
        }
    }
}