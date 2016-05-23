using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using terminalUtilities.Excel;
using TerminalBase.BaseClasses;
using Utilities;

namespace terminalExcel.Activities
{
    public class SetExcelTemplate_v1 : BaseTerminalActivity
    {
        private const string DataTableLabel = "Standard Data Table";

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SetExcelTemplate",
            Label = "Set Excel Template",
            Version = "1",
            MinPaneWidth = 330,
            Category = ActivityCategory.Processors,
            Terminal = TerminalData.TerminalDTO,
            Tags = "Table Data Generator,Skip At Run-Time",
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public readonly ControlDefinitionDTO select_file;

            public ActivityUi(string uploadedFileName = null, string uploadedFilePath = null)
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add((select_file = new ControlDefinitionDTO(ControlTypes.FilePicker)
                {
                    Label = "Select an Excel file",
                    Name = "select_file",
                    Required = true,
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onChange", "requestConfig")
                    },
                    Source = new FieldSourceDTO
                    {
                        Label = "Select an Excel file",
                        ManifestType = CrateManifestTypes.StandardConfigurationControls
                    },
                    Value = uploadedFilePath,
                }));

                if (!string.IsNullOrEmpty(uploadedFileName))
                {
                    Controls.Add(new TextBlock
                    {
                        Label = "",
                        Value = "Uploaded file: " + Uri.UnescapeDataString(uploadedFileName),
                        CssClass = "well well-lg"
                    });
                }

                Controls.Add(new TextBlock
                {
                    Label = "",
                    Value = "This Action will try to extract a table of rows from the first spreadsheet in the file.",
                    CssClass = "well well-lg TextBlockClass"
                });
            }
        }


        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public override async Task Run()
        {
            Success();
        }

        private StandardTableDataCM CreateStandardTableDataCM()
        {
            var uploadFilePath = GetUploadFilePath();
            string extension = Path.GetExtension(uploadFilePath);
            /*
            FileDTO curFileDO = new FileDTO()
            {
                CloudStorageUrl = uploadFilePath,
            };
            IFile file = ObjectFactory.GetInstance<IFile>();
            // Read file from repository
            var fileAsByteArray = file.Retrieve(curFileDO);*/

            return CreateStandardTableCMFromExcelFile(new byte[] {}/*fileAsByteArray*/, extension);
        }

        protected async Task<List<Crate<StandardFileDescriptionCM>>> GetUpstreamFileHandleCrates()
        {
            return await HubCommunicator.GetCratesByDirection<StandardFileDescriptionCM>(ActivityId, CrateDirection.Upstream, CurrentUserId);
        }

        //private async Task<StandardTableDataCM> GetUpstreamTableData()
        //{
        //    var upstreamFileHandleCrates = await GetUpstreamFileHandleCrates();

        //    //if no "Standard File Handle" crate found then return
        //    if (!upstreamFileHandleCrates.Any())
        //        return null;

        //    //if more than one "Standard File Handle" crates found then throw an exception
        //    if (upstreamFileHandleCrates.Count > 1)
        //        throw new Exception("More than one Standard File Handle crates found upstream.");

        //    // Deserialize the Standard File Handle crate to StandardFileHandleMS object
        //    StandardFileDescriptionCM fileHandleMS = upstreamFileHandleCrates.First().Get<StandardFileDescriptionCM>();

        //    // Use the url for file from StandardFileHandleMS and read from the file and transform the data into StandardTableData and assign it to Action's crate storage
        //    StandardTableDataCM tableDataMS = ExcelUtils.GetExcelFile(fileHandleMS.DirectUrl);

        //    return tableDataMS;
        //}


        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        public override async Task Initialize()
        {
            Storage.Clear();
            Storage.Add(PackControls(new ActivityUi()));
            Storage.Add(GetAvailableRunTimeTableCrate(DataTableLabel));
        }

        

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        public override async Task FollowUp()
        {
            var uploadFilePath = GetUploadFilePath();
            string fileName = null;
            if (!string.IsNullOrEmpty(uploadFilePath))
            {
                fileName = ExtractFileName(uploadFilePath);
            }
            else
            {
                var labelControl = Storage.CrateContentsOfType<StandardConfigurationControlsCM>()
                    .First()
                    .Controls
                    .FirstOrDefault(x => x.Value != null && x.Value.StartsWith("Uploaded file: "));

                if (labelControl != null)
                {
                    fileName = labelControl.Value.Substring("Uploaded file: ".Length);
                }
            }

            Storage.Remove<StandardConfigurationControlsCM>();
            Storage.Add(PackControls(new ActivityUi(fileName, uploadFilePath)));

            if (!string.IsNullOrEmpty(uploadFilePath))
            {
                var generatedTable = CreateStandardTableDataCM();
                var tableCrate = Crate.FromContent(DataTableLabel, generatedTable, AvailabilityType.Always);
                Storage.Add(tableCrate);
            }
        }

        private Crate GetAvailableRunTimeTableCrate(string descriptionLabel)
        {
            var availableRunTimeCrates = Crate.FromContent(DataTableLabel, new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                        Label = descriptionLabel,
                        ManifestId = (int)MT.StandardTableData,
                        ProducedBy = "SetExcelTemplate_v1"
                    }), AvailabilityType.Always);

            return availableRunTimeCrates;
        }

        private string ExtractFileName(string uploadFilePath)
        {
            if (uploadFilePath == null)
            {
                return null;
            }

            var index = uploadFilePath.LastIndexOf('/');
            if (index >= 0 && (uploadFilePath.Length > index + 1))
            {
                return uploadFilePath.Substring(index + 1);
            }

            return uploadFilePath;
        }

        private StandardTableDataCM CreateStandardTableCMFromExcelFile(byte[] excelFile, string excelFileExtension)
        {
            var rowsDictionary = ExcelUtils.GetTabularData(excelFile, excelFileExtension, false);
            if (rowsDictionary != null)
            {
                var rows = ExcelUtils.CreateTableCellPayloadObjects(rowsDictionary);
                if (rows != null)
                {
                    return new StandardTableDataCM
                    {
                        FirstRowHeaders = false,
                        Table = rows
                    };
                }
            }

            return null;
        }

        private string GetUploadFilePath()
        {

            var filePathsFromUserSelection = Storage.CrateContentsOfType<StandardConfigurationControlsCM>()
                .Select(x =>
                {
                    var actionUi = new ActivityUi();
                    actionUi.ClonePropertiesFrom(x);
                    return actionUi;
                })
                 .Where(x => !string.IsNullOrEmpty(x.select_file.Value)).ToArray();

            if (filePathsFromUserSelection.Length > 1)
            {
                throw new Exception("AmbiguityException");
            }

            string uploadFilePath = null;
            if (filePathsFromUserSelection.Length > 0)
            {
                uploadFilePath = filePathsFromUserSelection[0].select_file.Value;
            }

            return uploadFilePath;
        }

        public SetExcelTemplate_v1() : base(false)
        {
        }
    }
}