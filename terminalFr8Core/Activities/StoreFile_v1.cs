using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Hub.Managers;
using StructureMap.Diagnostics;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class StoreFile_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "StoreFile",
            Label = "Store File",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            Type = ActivityType.Standard,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
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

        public async Task UpdateUpstreamFileCrates()
        {
            // Build a crate with the list of available upstream fields
            var curUpstreamFieldsCrate = Storage.SingleOrDefault(c => c.ManifestType.Id == (int)MT.FieldDescription
                                                                                && c.Label == "Upstream Terminal-Provided File Crates");
            if (curUpstreamFieldsCrate != null)
            {
                Storage.Remove(curUpstreamFieldsCrate);
            }

            var upstreamFileCrates = await HubCommunicator.GetCratesByDirection<StandardFileDescriptionCM>(ActivityId, CrateDirection.Upstream);

            var curUpstreamFields = upstreamFileCrates.Select(c => new FieldDTO(c.Label, c.Label)).ToArray();

            curUpstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided File Crates", curUpstreamFields);
            Storage.Add(curUpstreamFieldsCrate);
            
        }

        private Crate CreateControlsCrate()
        {
            var fileNameTextBox = new TextBox
            {
                Label = "Name of file",
                Name = "File_Name"
            };
            var textSource = new TextSource("File Crate Label", "Upstream Terminal-Provided File Crates", "File Crate label");
            return PackControlsCrate(fileNameTextBox, textSource);
        }

        public StoreFile_v1(ICrateManager crateManager)
            : base(false, crateManager)
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
            await UpdateUpstreamFileCrates();
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}