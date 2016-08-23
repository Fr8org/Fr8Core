using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using RestSharp.Extensions;

namespace terminalWord.Activities
{
    public class Load_Word_File_v1 : TerminalActivity<Load_Word_File_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("8BBB1FE4-58F0-4342-B0E2-65CECB21F5DC"),
            Name = "Load_Word_File",
            Label = "Load Word File",
            Version = "1",
            MinPaneWidth = 300,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string FileCrateLabel = "File uploaded by Load Word";

        private const string ConfigurationCrateLabel = "Selected File";

        private const string ExternalObjectHandlesLabel = "External Object Handles";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public FilePicker FilePicker { get; set; }

            public TextBlock UploadedFileDescription { get; set; }

            public TextBlock ActivityDescription { get; set; }

            public ActivityUi()
            {
                FilePicker = new FilePicker
                {
                    FileExtensions= ".doc,.docx",
                    Label = "Select a Word file",
                    Name = nameof(FilePicker),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                };
                Controls.Add(FilePicker);
                UploadedFileDescription = new TextBlock
                {
                    Name = nameof(UploadedFileDescription),
                    IsHidden = true
                };
                Controls.Add(UploadedFileDescription);
            }
            public void MarkFileAsUploaded(string fileName, string filePath)
            {
                FilePicker.Value = filePath;
                UploadedFileDescription.Value = $"Uploaded file: {fileName}";
                UploadedFileDescription.IsHidden = false;
            }

            public void ClearFileDescription()
            {
                UploadedFileDescription.Value = string.Empty;
                UploadedFileDescription.IsHidden = true;
            }
        }
        private readonly Fr8.Infrastructure.Interfaces.IRestfulServiceClient _restfulServiceClient;
        public Load_Word_File_v1(ICrateManager crateManager, Fr8.Infrastructure.Interfaces.IRestfulServiceClient restfulServiceClient)
            : base(crateManager)
        {
            _restfulServiceClient = restfulServiceClient;
        }

        public override async Task Initialize()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardFileDescriptionCM>(FileCrateLabel);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if(string.IsNullOrEmpty(ActivityUI.FilePicker.Value))
            {
                ValidationManager.SetError("File must be specified", ActivityUI.FilePicker);
            }
            return Task.FromResult(0);
        }

        public async Task<byte[]> GetFileAsByteArray(string selectedFilePath)
        {
            var fileAsByteArray = await RetrieveFile(selectedFilePath);
            fileAsByteArray.Position = 0;
            return fileAsByteArray.ReadAsBytes();
        }
        private async Task<System.IO.Stream> RetrieveFile(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);
            if (ext != ".doc" && ext != ".docx")
            {
                throw new ArgumentException("Expected '.doc' or '.docx'", "selectedFile");
            }
            return await _restfulServiceClient.DownloadAsync(new Uri(filePath));
        }

        public override async Task Run()
        {
            var byteArray = await GetFileAsByteArray(ActivityUI.FilePicker.Value);

            var fileDescription = new StandardFileDescriptionCM
            {

                TextRepresentation = Convert.ToBase64String(byteArray),
                Filetype = System.IO.Path.GetExtension(ActivityUI.FilePicker.Value),
                Filename = System.IO.Path.GetFileName(ActivityUI.FilePicker.Value)
            };

            Payload.Add(Crate.FromContent(FileCrateLabel, fileDescription));
        }
    }
}