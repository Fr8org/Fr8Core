using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StructureMap;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using Utilities;
using System.IO;
using Utilities.Interfaces;

namespace pluginExcel.Actions
{
    public class ExtractData_v1 : BasePluginAction
    {

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public ActionProcessResultDTO Execute(FileDO fileDO)
        {
            //var curFieldMappingSettings = fileDO.CrateStorageDTO()
            //    .CrateDTO
            //    .Where(x => x.Label == "Field Mappings")
            //    .FirstOrDefault();

            //if (curFieldMappingSettings == null)
            //{
            //    throw new ApplicationException("No Field Mapping cratefound for current action.");
            //}

            //var curFieldMappingJson = JsonConvert.SerializeObject(curFieldMappingSettings, JsonSettings.CamelCase);

            //var crates = new List<CrateDTO>()
            //{
            //    new CrateDTO()
            //    {
            //        Contents = curFieldMappingJson,
            //        Label = "Payload",
            //        ManifestType = "Standard Payload Data"
            //    }
            //};

            //((ActionListDO)fileDO.ParentActivity).Process.UpdateCrateStorageDTO(crates);

            return new ActionProcessResultDTO() { Success = true };
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public ActionDTO Configure(ActionDTO actionDTO)
        {
            return ProcessConfigurationRequest(actionDTO, ConfigurationEvaluator);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private CrateDTO CreateStandardConfigurationControls()
        {
            var fieldFilterPane = new FilterPaneFieldDefinitionDTO()
            {
                FieldLabel = "Select Excel File",
                Type = "filePicker",
                Name = "select_file",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Select Excel File",
                    ManifestType = CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
                }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            CrateDTO getErrorMessageCrate = null; 

            ActionDO curActionDO = _action.MapFromDTO(curActionDTO);

            var curUpstreamFields = GetDesignTimeFields(curActionDO, GetCrateDirection.Upstream).Fields.ToArray();

            var curDownstreamFields = GetDesignTimeFields(curActionDO, GetCrateDirection.Downstream).Fields.ToArray();

            if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
            {
                getErrorMessageCrate = GetTextBoxControlForDisplayingError("MapFieldsErrorMessage",
                         "This action couldn't find either source fields or target fields (or both). " +
                        "Try configuring some Actions first, then try this page again.");
                curActionDTO.CurrentView = "MapFieldsErrorMessage";
            }

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            CrateDTO downstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Downstream Plugin-Provided Fields", curDownstreamFields);
            CrateDTO upstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);

            var curConfigurationControlsCrate = CreateStandardConfigurationControls();

            curActionDTO.CrateStorage = AssembleCrateStorage(downstreamFieldsCrate, upstreamFieldsCrate, curConfigurationControlsCrate, getErrorMessageCrate);
            return curActionDTO;

        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name select_file with a value
            var controlsCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);
            var filePaths = _crate.GetElementByKey(controlsCrates, key: "select_file", keyFieldName: "name")
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            //if there are more than 2 return file names, something is wrong
            //if there are none or if there's one but it's value is "" the return initial else return followup
            var objCount = filePaths.Length;
            if (objCount > 1)
                throw new ArgumentException("didn't expect to see more than one file names with the name select_file on this Action", "curActionDTO");
            if (objCount == 0)
                return ConfigurationRequestType.Initial;
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        //Returning the crate with text field control 
        private CrateDTO GetTextBoxControlForDisplayingError(string fieldLabel, string errorMessage)
        {
            var fields = new List<FieldDefinitionDTO>() 
            {
                new TextBlockFieldDTO()
                {
                    FieldLabel = fieldLabel,
                    Value = errorMessage,
                    Type = "textBlockField",
                    cssClass = "well well-lg"
                    
                }
            };

            var controls = new StandardConfigurationControlsMS()
            {
                Controls = fields
            };

            var crateControls = _crate.Create(
                        "Configuration_Controls",
                        JsonConvert.SerializeObject(controls),
                        "Standard Configuration Controls"
                    );

            return crateControls;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //In all followup calls, update data fields of the configuration store          
            List<String> contentsList = GetHeadersFromExcel(curActionDTO);

            var curCrateStorageDTO = new CrateStorageDTO
            {
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                CrateDTO = new List<CrateDTO>
                {
                    _crate.CreateDesignTimeFieldsCrate(
                        "Spreadsheet Column Headers",
                        contentsList.Select(col => new FieldDTO() { Key = col, Value = col }).ToArray()
                    )
                }
            };

            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);

            int foundSameCrateDTOAtIndex = curActionDO.CrateStorageDTO().CrateDTO.FindIndex(m => m.Label == "Spreadsheet Column Headers");
            if (foundSameCrateDTOAtIndex == -1)
            {
                _action.AddCrate(curActionDO, curCrateStorageDTO.CrateDTO.ToList());
            }
            else
            {
                CrateStorageDTO localList = curActionDO.CrateStorageDTO();
                localList.CrateDTO.RemoveAt(foundSameCrateDTOAtIndex);
                curActionDO.CrateStorage = JsonConvert.SerializeObject(localList);
                _action.AddCrate(curActionDO, curCrateStorageDTO.CrateDTO.ToList());
            }
            curCrateStorageDTO = curActionDO.CrateStorageDTO();
            curActionDTO.CrateStorage = curCrateStorageDTO;
            return curActionDTO;
        }

        private List<string> GetHeadersFromExcel(ActionDTO curActionDTO)
        {
            var controlsCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);

            var filePaths = _crate.GetElementByKey(controlsCrates, key: "select_file", keyFieldName: "name")
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            var excelFilePath = filePaths[0];
            string csvFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";

            try
            {
                ExcelUtils.ConvertToCsv(excelFilePath, csvFilePath);
                using (ICsvReader csvReader = new CsvReader(csvFilePath))
                {
                    var columns = csvReader.GetColumnHeaders();
                    return columns.ToList<string>();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                try { File.Delete(csvFilePath); }
                catch { }
            }

            return new List<string>();
        }
    }
}