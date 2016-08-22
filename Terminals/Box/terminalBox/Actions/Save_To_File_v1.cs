using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalBox.Infrastructure;
using System;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Utilities;

namespace terminalBox.Actions
{
    public class Save_To_File_v1 : TerminalActivity<Save_To_File_v1.ActivityUi>
    {
        private readonly IPushNotificationService _pushNotificationService;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("774277C1-E26D-405E-AC5B-9415F90369BF"),
            Name = "Save_To_File",
            Label = "Save To File",
            Version = "1",
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser FileChooser { get; set; }
            public TextBox Filename { get; set; }

            public ActivityUi(UiBuilder uiBuilder)
            {
                Controls.Add(FileChooser = uiBuilder.CreateCrateChooser(
                    "FileChooser", 
                    "Select file to store:", 
                    requestConfig: true, 
                    allowedManifestTypes: new[] { MT.StandardTableData.GetEnumDisplayName()}));
                Controls.Add(Filename = new TextBox
                {
                    Name = "Filename",
                    Label = "Enter the file name: "
                });
            }
        }

        public Save_To_File_v1(ICrateManager crateManager, IPushNotificationService pushNotificationService) 
            : base(crateManager)
        {
            _pushNotificationService = pushNotificationService;
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if (!ActivityUI.FileChooser.CrateDescriptions.Any(x => x.Selected))
            {
                ValidationManager.SetError("Data to save is not specified", ActivityUI.FileChooser.Name);
            }
            if (string.IsNullOrWhiteSpace(ActivityUI.Filename.Value))
            {
                ValidationManager.SetError("File name can't be empty", ActivityUI.Filename.Name);
            }
            return base.Validate();
        }

        public override async Task Run()
        {
            var token = JsonConvert.DeserializeObject<BoxAuthTokenDO>(AuthorizationToken.Token);
            var desiredCrateDescription = ActivityUI.FileChooser.CrateDescriptions.Single(x => x.Selected);
            var tableCrate = Payload.CratesOfType<StandardTableDataCM>()
                .FirstOrDefault(x => x.Label == desiredCrateDescription.Label 
                && x.ManifestType.Type == desiredCrateDescription.ManifestType);

            if (tableCrate == null)
            {
                RaiseError($"Selected crate {desiredCrateDescription.Label} doesn't contains table data");
                return;
            }
            var fileName = ActivityUI.Filename.Value.Trim();
            var service = new BoxService(token);
            string fileId;
            using (var stream = new MemoryStream())
            {
                CreateWorkbook(stream, tableCrate);
                // Need to reset stream before saving it to box.
                stream.Seek(0, SeekOrigin.Begin);
                fileId = await service.SaveFile($"{fileName}.xlsx", stream);
            }
            var downloadLink = await service.GetFileLink(fileId);
            await _pushNotificationService.PushUserNotification(MyTemplate, "File Download URL Generated", "File was upload to Box. You can download it using this url: " + downloadLink);
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
                    foreach (var cell in content.Table[i].Row)
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