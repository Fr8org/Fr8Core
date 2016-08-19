using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using System;
using Fr8.TerminalBase.Services;
using System.Collections.Generic;
using Fr8.Infrastructure.Utilities;

namespace terminalFr8Core.Activities
{
    public class Store_File_v1 : TerminalActivity<Store_File_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("1c4f979d-bc1c-4a4a-b370-049dbacd3678"),
            Name = "Store_File",
            Label = "Store File",
            Version = "1",
            MinPaneWidth = 330,
            Type = ActivityType.Standard,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private MemoryStream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser UpstreamCrateChooser { get; set; }

            public TextBox FileNameTextBox { get; set; }

            public ActivityUi(UiBuilder builder)
            {
                UpstreamCrateChooser = new CrateChooser
                {
                    Label = "Select a File",
                    Name = "file_selector",
                    Events = new List<ControlEvent>
                    {
                        ControlEvent.RequestConfig
                    },
                    Required = true,
                    AllowedManifestTypes= new[] { MT.StandardFileHandle.GetEnumDisplayName() },
                    SingleManifestOnly = true,
                    RequestUpstream = true
                };

                Controls.Add(UpstreamCrateChooser);

                FileNameTextBox = new TextBox
                {
                    Label = "Name of file",
                    Name = "File_Name"
                };
                Controls.Add(FileNameTextBox);
            }
        }

        public Store_File_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override async Task Run()
        {
            if (string.IsNullOrEmpty(ActivityUI.FileNameTextBox.Value))
            {
                RaiseError("No file name was given on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }

            //we should upload this file to our file storage
            var userSelectedFileManifest = Payload.CrateContentsOfType<StandardFileDescriptionCM>().FirstOrDefault();
            if (userSelectedFileManifest == null)
            {
                RaiseError("No StandardFileDescriptionCM Crate was found", ActivityErrorCode.PAYLOAD_DATA_MISSING);
            }

            var fileContents = userSelectedFileManifest.TextRepresentation;

            using (var stream = GenerateStreamFromString(fileContents))
            {
                //TODO what to do with this fileDO??
                var fileDO = await HubCommunicator.SaveFile(ActivityUI.FileNameTextBox.Value, stream);
            }
            Success();
        }

        public override Task Initialize()
        {
            //build a controls crate to render the pane
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}