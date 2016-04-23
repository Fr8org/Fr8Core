using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Exceptions;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalUtilities.Excel;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalExcel.Activities
{
    public class SetExcelTemplate_v1 : BaseTerminalActivity
    {

        private const string DataTableLabel = "Standard Data Table";
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
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        private StandardTableDataCM CreateStandardTableDataCM(ICrateStorage storage)
        {
            var uploadFilePath = GetUploadFilePath(storage);
            string extension = Path.GetExtension(uploadFilePath);
            FileDO curFileDO = new FileDO()
            {
                CloudStorageUrl = uploadFilePath,
            };

            IFile file = ObjectFactory.GetInstance<IFile>();
            // Read file from repository
            var fileAsByteArray = file.Retrieve(curFileDO);

            return CreateStandardTableCMFromExcelFile(fileAsByteArray, extension);
        }

        private async Task<StandardTableDataCM> GetUpstreamTableData(ActivityDO activityDO)
        {
            var upstreamFileHandleCrates = await GetUpstreamFileHandleCrates(activityDO);

            //if no "Standard File Handle" crate found then return
            if (!upstreamFileHandleCrates.Any())
                return null;

            //if more than one "Standard File Handle" crates found then throw an exception
            if (upstreamFileHandleCrates.Count > 1)
                throw new Exception("More than one Standard File Handle crates found upstream.");

            // Deserialize the Standard File Handle crate to StandardFileHandleMS object
            StandardFileDescriptionCM fileHandleMS = upstreamFileHandleCrates.First().Get<StandardFileDescriptionCM>();

            // Use the url for file from StandardFileHandleMS and read from the file and transform the data into StandardTableData and assign it to Action's crate storage
            StandardTableDataCM tableDataMS = ExcelUtils.GetExcelFile(fileHandleMS.DockyardStorageUrl);

            return tableDataMS;
        }


        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(GetAvailableRunTimeTableCrate(DataTableLabel));
            }

            return curActivityDO;
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var storage = CrateManager.GetStorage(curActivityDO);
            var uploadFilePath = GetUploadFilePath(storage);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                string fileName = null;
                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    fileName = ExtractFileName(uploadFilePath);
                }
                else
                {
                    var labelControl = storage.CrateContentsOfType<StandardConfigurationControlsCM>()
                        .First()
                        .Controls
                        .FirstOrDefault(x => x.Value != null && x.Value.StartsWith("Uploaded file: "));

                    if (labelControl != null)
                    {
                        fileName = labelControl.Value.Substring("Uploaded file: ".Length);
                    }
                }

                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Add(PackControls(new ActivityUi(fileName, uploadFilePath)));

                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    var generatedTable = CreateStandardTableDataCM(storage);
                    var tableCrate = Crate.FromContent(DataTableLabel, generatedTable, AvailabilityType.Always);
                    crateStorage.Add(tableCrate);
                }
            }

            return curActivityDO;
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

        private string GetUploadFilePath(ICrateStorage storage)
        {

            var filePathsFromUserSelection = storage.CrateContentsOfType<StandardConfigurationControlsCM>()
                .Select(x =>
                {
                    var actionUi = new ActivityUi();
                    actionUi.ClonePropertiesFrom(x);
                    return actionUi;
                })
                 .Where(x => !string.IsNullOrEmpty(x.select_file.Value)).ToArray();

            if (filePathsFromUserSelection.Length > 1)
            {
                throw new AmbiguityException();
            }

            string uploadFilePath = null;
            if (filePathsFromUserSelection.Length > 0)
            {
                uploadFilePath = filePathsFromUserSelection[0].select_file.Value;
            }

            return uploadFilePath;
        }
    }
}