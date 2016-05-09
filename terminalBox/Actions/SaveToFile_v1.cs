using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using terminalBox.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalBox.Actions
{
    public class SaveToFile_v1 : EnhancedTerminalActivity<SaveToFile_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser FileChooser { get; set; }
            public TextBox Filename { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                Controls.Add(FileChooser = uiBuilder.CreateCrateChooser("FileChooser", "Select file to store:", true, true));
                /*Controls.Add(Filename = new DropDownList()
                {
                    Label = "Select where to store file",
                    Name = "Filename"
                });*/

                Controls.Add(Filename = new TextBox()
                { Name = "Filename", Label = "Enter the file name: "});
            }
        }

        public SaveToFile_v1() 
            : base(true)
        {
        }

        /*private async Task FillAvailableFolders()
        {
            var token = JsonConvert.DeserializeObject<BoxAuthTokenDO>(AuthorizationToken.Token);
            var folders = await new BoxService().ListFolders(token);

            ConfigurationControls.Filename.ListItems = folders.Select(x => new ListItem()
            {
                Key = x.Value,
                Value = x.Key
            }).ToList();
        }*/

        protected override async Task Initialize(CrateSignaller crateSignaller)
        {
           //await FillAvailableFolders();
        }

        protected override async Task Configure(CrateSignaller crateSignaller)
        {
            //await FillAvailableFolders();
        }

        protected override async Task RunCurrentActivity()
        {
            var token = JsonConvert.DeserializeObject<BoxAuthTokenDO>(AuthorizationToken.Token);
            var desiredCrateDescription = ConfigurationControls.FileChooser.CrateDescriptions.Single(x => x.Selected);
            var tableCrate =  CurrentPayloadStorage.CratesOfType<StandardTableDataCM>().FirstOrDefault(x => x.Label == desiredCrateDescription.Label && x.ManifestType.Type == desiredCrateDescription.ManifestType);

            if (tableCrate == null)
            {
                Error($"Selected crate {desiredCrateDescription.Label} doesn't contains table data");
                return;
            }
            var fileName = ConfigurationControls.Filename.Value;

            //if (!int.TryParse(tableCrate.Content.Filename, NumberStyles.Any, CultureInfo.InvariantCulture, out fileId))
            //{
            //    Error($"Corrupted file info in crate {desiredCrateDescription.Label}");
            //    return;
            //}

            //var stream = await HubCommunicator.DownloadFile(fileId, CurrentFr8UserId);
            //var mem = new MemoryStream();

            //stream.CopyTo(mem);

            //string fileName = ConfigurationControls.Filename.Value;

            //if (string.IsNullOrWhiteSpace(fileName))
            //{
            //    if (string.IsNullOrWhiteSpace(tableCrate.Content.TextRepresentation))
            //    {
            //        fileName = Guid.NewGuid().ToString("N");
            //    }
            //    else
            //    {
            //        fileName = tableCrate.Content.TextRepresentation;
            //    }
            //}
            //var extSeparator = fileName.LastIndexOf('.');

            //if (extSeparator < 0 && !string.IsNullOrWhiteSpace(tableCrate.Content.Filetype))
            //{
            //    fileName = fileName + tableCrate.Content.Filetype;
            //}
            var service = new BoxService(token);
            var fileId = service.SaveFile(fileName, new MemoryStream()).Result;
            var downloadLink = service.GetFileLink(fileId);


            await HubCommunicator.NotifyUser(new TerminalNotificationDTO
            {
                Type = "Success",
                ActivityName = "SaveToFile",
                ActivityVersion = "1",
                TerminalName = "terminalBox",
                TerminalVersion = "1",
                Message = "File was upload to Box. You can download it using this url: " + downloadLink,
                Subject = "File download URL"
            }, CurrentFr8UserId);
        }
    }
}