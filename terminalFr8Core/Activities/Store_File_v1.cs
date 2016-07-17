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

namespace terminalFr8Core.Activities
{
    public class Store_File_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("1c4f979d-bc1c-4a4a-b370-049dbacd3678"),
            Name = "Store_File",
            Label = "Store File",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            Type = ActivityType.Standard,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
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
        
        private Crate CreateControlsCrate()
        {
            var fileNameTextBox = new TextBox
            {
                Label = "Name of file",
                Name = "File_Name"
            };
            var textSource = new TextSource("File Crate Label", null, "File Crate label");
            return PackControlsCrate(fileNameTextBox, textSource);
        }

        public Store_File_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override async Task Run()
        {
            var textSourceControl = GetControl<TextSource>("File Crate label");
            var fileNameField = GetControl<TextBox>("File_Name");
            var fileCrateLabel = textSourceControl.GetValue(Payload);
            if (string.IsNullOrEmpty(fileCrateLabel))
            {
                RaiseError("No Label was selected on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }
            if (string.IsNullOrEmpty(fileNameField.Value))
            {
                RaiseError("No file name was given on design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }


            //we should upload this file to our file storage
            var userSelectedFileManifest = Payload.CrateContentsOfType<StandardFileDescriptionCM>(f => f.Label == fileCrateLabel).FirstOrDefault();
            if (userSelectedFileManifest == null)
            {
                RaiseError("No StandardFileDescriptionCM Crate was found with label " + fileCrateLabel, ActivityErrorCode.PAYLOAD_DATA_MISSING);
            }


            var fileContents = userSelectedFileManifest.TextRepresentation;

            using (var stream = GenerateStreamFromString(fileContents))
            {
                //TODO what to do with this fileDO??
                var fileDO = await HubCommunicator.SaveFile(fileNameField.Value, stream);
            }


            Success();
        }

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
           // await UpdateUpstreamFileCrates();
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}