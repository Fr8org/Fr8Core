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
using System;

namespace terminalFr8Core.Actions
{
    public class Get_File_From_Fr8_Store_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("82a722b5-40a6-42d7-8296-aa5239f10173"),
            Name = "Get_File_From_Fr8_Store",
            Label = "Get File From Fr8 Store",
            Version = "1",
            Type = ActivityType.Standard,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private void CreateControls()
        {
            var fileSelectionDropdown = new DropDownList
            {
                Label = "Please select file to get from Fr8 Store",
                Name = "FileSelector",
                Source = null
            };

            AddControls(fileSelectionDropdown);
        }

        #region Fill Source
        private async Task FillFileSelectorSource(string controlName)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);

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

            string textRepresentation;
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
            CreateControls();
            await FillFileSelectorSource("FileSelector");
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}