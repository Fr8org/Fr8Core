using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
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
                Controls.Add(Filename = new TextBox()
                { Name = "Filename", Label = "Enter the file name: " });
            }
        }

        public SaveToFile_v1()
            : base(true)
        {
        }

        protected override async Task Initialize(CrateSignaller crateSignaller)
        {
            await Task.Yield();
        }

        protected override async Task Configure(CrateSignaller crateSignaller)
        {
            await Task.Yield();
        }

        protected override async Task RunCurrentActivity()
        {
            var token = JsonConvert.DeserializeObject<BoxAuthTokenDO>(AuthorizationToken.Token);
            var desiredCrateDescription = ConfigurationControls.FileChooser.CrateDescriptions.Single(x => x.Selected);
            var tableCrate = CurrentPayloadStorage.CratesOfType<StandardTableDataCM>()
                .FirstOrDefault(x => x.Label == desiredCrateDescription.Label 
                && x.ManifestType.Type == desiredCrateDescription.ManifestType);

            if (tableCrate == null)
            {
                Error($"Selected crate {desiredCrateDescription.Label} doesn't contains table data");
                return;
            }
            var fileName = ConfigurationControls.Filename.Value;
            var service = new BoxService(token);
            string fileId;
            using (var stream = new MemoryStream())
            {
                CreateWorkbook(stream, tableCrate);
                // Need to reset stream before saving it to box.
                stream.Seek(0, SeekOrigin.Begin);
                fileId = service.SaveFile(fileName + ".xlsx", stream).Result;
            }
            var downloadLink = service.GetFileLink(fileId).Result;

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

        /// <summary>
        /// Creates a workbook from <paramref name="tableCrate"/> and saves it into <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">Stream to save data</param>
        /// <param name="tableCrate">Crate with data</param>
        public void CreateWorkbook(MemoryStream stream, Crate<StandardTableDataCM> tableCrate)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TableData");
            var content = tableCrate.Content;
            // In Excel all row/column indexers starts from 1.
            if (tableCrate.Content.HasDataRows)
            {
                for (int i = 0; i < content.Table.Count; i++)
                {
                    int j = 0;
                    foreach (TableCellDTO cell in content.Table[i].Row)
                    {
                        j++;
                        worksheet.Cell(i + 1, j).Value = cell.Cell.Value;
                    }
                }
            }
            workbook.SaveAs(stream);
        }
    }
}