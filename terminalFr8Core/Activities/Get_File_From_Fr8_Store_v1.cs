using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalFr8Core.Actions
{
    public class Get_File_From_Fr8_Store_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Get_File_From_Fr8_Store",
            Label = "Get File From Fr8 Store",
            Category = ActivityCategory.Receivers,
            Version = "1",
            Type = ActivityType.Standard,
            MinPaneWidth = 330,
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

        public Get_File_From_Fr8_Store_v1(ICrateManager crateManager)
            : base(crateManager)
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
            configurationCrate.Availability = AvailabilityType.Always;
            Storage.Add(configurationCrate);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}