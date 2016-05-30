using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using System.IO;
using System.Text;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalFr8Core.Actions
{
    public class GetFileFromFr8Store_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "GetFileFromFr8Store",
            Label = "Get File From Fr8 Store",
            Category = ActivityCategory.Receivers,
            Version = "1",
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

        private Crate CreateControlsCrate()
        {
            var fileSelectionDropdown = new DropDownList
            {
                Label = "Please select file to get from Fr8 Store",
                Name = "FileSelector",
                Source = null
            };

            return PackControlsCrate(fileSelectionDropdown);
        }

        #region Fill Source
        private async Task FillFileSelectorSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = await GetCurrentUsersFiles();
            }
        }

        private async Task<List<ListItem>> GetCurrentUsersFiles()
        {
            var curAccountFileList = await HubCommunicator.GetFiles();
            //TODO where tags == Docusign files
            return curAccountFileList.Select(c => new ListItem() { Key = c.OriginalFileName, Value = c.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
        }
        #endregion

        public GetFileFromFr8Store_v1(ICrateManager crateManager)
            : base(false, crateManager)
        {
        }

        public override async Task Run()
        {
            var fileSelector = GetControl<DropDownList>("FileSelector");
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

            string textRepresentation = string.Empty;
            using (var reader = new StreamReader(file, Encoding.UTF8))
            {
                textRepresentation = reader.ReadToEnd();
            }

            //let's convert this file to a string and store it in a file crate
            var fileDescription = new StandardFileDescriptionCM
            {
                Filename = fileSelector.selectedKey,
                TextRepresentation = textRepresentation
            };
            var fileCrate = Crate.FromContent("DownloadFile", fileDescription);
            Payload.Add(fileCrate);
            Success();
        }

        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            var configurationCrate = CreateControlsCrate();
            await FillFileSelectorSource(configurationCrate, "FileSelector");
            Storage.Add(configurationCrate);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}