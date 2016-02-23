using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Hub.Services;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{

    public class ConvertRelatedFieldsIntoTable_v1 : BaseTerminalActivity
    {
        private const string FirstIntegerRegexPattern = "\\d+";

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authToken)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authToken);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActionDataPackageDO)
        {
            if (CrateManager.IsStorageEmpty(curActionDataPackageDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var storage = CrateManager.GetStorage(curActionDataPackageDO);
            var hasControlsCrate = GetConfigurationControls(storage) != null;

            var hasManifestTypeList = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Manifest Type List").Any();

            if (hasControlsCrate && hasManifestTypeList)
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;

        }

        /// <summary>
        /// Looks for first Create with Id == "Standard Design-Time" among all upcoming Actions.
        /// </summary>
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActivityDO.Id != Guid.Empty)
            {
                //build a controls crate to render the pane
                var configurationControlsCrate = PackCrate_ConfigurationControls();

                using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
                {
                    crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                    crateStorage.Add(await GetUpstreamManifestTypes(curActivityDO));
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }

            return curActivityDO;
        }


        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = GetConfigurationControls(curActivityDO);
            var upstreamDataChooser = (UpstreamDataChooser)controlsMS.Controls.Single(x => x.Type == ControlTypes.UpstreamDataChooser && x.Name == "Upstream_data_chooser");
            if (upstreamDataChooser.SelectedManifest != null)
            {
                var labelList = await GetLabelsByManifestType(curActivityDO, upstreamDataChooser.SelectedManifest);

                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.RemoveByLabel("Upstream Crate Label List");
                    crateStorage.Add(Data.Crates.Crate.FromContent("Upstream Crate Label List", new FieldDescriptionsCM() { Fields = labelList }));
                }
            }

            return curActivityDO;
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            var storage = CrateManager.GetStorage(curPayloadDTO);

            var designTimeControls = GetConfigurationControls(curActivityDO);
            var upstreamDataChooser = (UpstreamDataChooser)designTimeControls.Controls.Single(x => x.Type == ControlTypes.UpstreamDataChooser && x.Name == "Upstream_data_chooser");
            
            var filteredCrates = storage.Where(s => true);
            
            //add filtering according to upstream data chooser
            if (upstreamDataChooser.SelectedManifest != null)
            {
                filteredCrates = filteredCrates.Where(s => s.ManifestType.Type == upstreamDataChooser.SelectedManifest);
            }
            if (upstreamDataChooser.SelectedLabel != null)
            {
                filteredCrates = filteredCrates.Where(s => s.Label == upstreamDataChooser.SelectedLabel);
            }

            var fieldList = CrateManager.GetFields(filteredCrates);

            
            if (upstreamDataChooser.SelectedFieldType != null)
            {
                //not quite sure what to do with this
                fieldList = fieldList.Where(s => s.Tags == upstreamDataChooser.SelectedFieldType);
            }


            var prefixValue = GetRowPrefix(designTimeControls);
            if (prefixValue == null)
            {
                return Error(curPayloadDTO/*, "This action can't run without a selected column prefix"*/);
            }

            var fieldsWithPrefix = ExtractFieldsContainingPrefix(fieldList, prefixValue);


            //we get fields that match our conditions
            //and convert them to TableRowDTO
            var rows = fieldsWithPrefix
                //select each field with their row number
                //and remove prefix part
                // Line4Expense is converted to lineNumber:4 field: {Key: Expense, Value: xxx}
                .Select(f => new
                {
                    lineNumber = Int32.Parse(Regex.Match(f.Key, FirstIntegerRegexPattern).Value),
                    field = new FieldDTO(f.Key.Substring(f.Key.IndexOf(Regex.Match(f.Key, FirstIntegerRegexPattern).Value, StringComparison.Ordinal)+1), f.Value)
                })
                //group by linenumber to prepare a table
                .GroupBy(r => r.lineNumber)
                .Select(r => new TableRowDTO
                {
                    Row = r.Select(f => new TableCellDTO
                    {
                        Cell = f.field
                    }).ToList()
                });

            var tableDataCrate = CrateManager.CreateStandardTableDataCrate("AssembledTableData", false, rows.ToArray());
            using (var crateStorage = CrateManager.GetUpdatableStorage(curPayloadDTO))
            {
                crateStorage.Add(tableDataCrate);
            }

            return Success(curPayloadDTO);
        }

        private IEnumerable<FieldDTO> ExtractFieldsContainingPrefix(IEnumerable<FieldDTO> fields, String prefix)
        {
            //this regex searchs for strings that start with user typed prefix value
            //and continues with a number example: "Line52_Expense", "Line4" are valid
            var matchRegex = new Regex("^(" + prefix + ")[0-9]+");
            return fields.Where(f => matchRegex.IsMatch(f.Key));
        }

        private string GetRowPrefix(StandardConfigurationControlsCM configurationControlsCM)
        {
            return configurationControlsCM.Controls.Single(c => c.Name == "Selected_Table_Prefix").Value;            
        }

        private async Task<Crate> GetUpstreamManifestTypes(ActivityDO curActivityDO)
        {
            var upstreamCrates = await GetCratesByDirection(curActivityDO, CrateDirection.Upstream);
            var manifestTypeOptions = upstreamCrates.GroupBy(c => c.ManifestType).Select(c => new FieldDTO(c.Key.Type, c.Key.Type));
            var queryFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Manifest Type List", manifestTypeOptions.ToArray());
            return queryFieldsCrate;
        }

        private async Task<List<FieldDTO>> GetLabelsByManifestType(ActivityDO curActivityDO, string manifestType)
        {
            var upstreamCrates = await GetCratesByDirection(curActivityDO, CrateDirection.Upstream);
            return CrateManager.GetLabelsByManifestType(upstreamCrates, manifestType).Select(c => new FieldDTO(c, c)).ToList();
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var actionExplanation = new TextBlock()
            {
                Label = "This Action converts Unstructured Field data from Upstream to Standard Table data that can be more effectively manipulated.",
                Name = "Field_Explanation",
            };
            var upstreamDataChooser = new UpstreamDataChooser
            {
                Name = "Upstream_data_chooser",
                Label = "Please select data type",
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };
            var fieldSelectPrefix = new TextBox()
            {
                Label = "All field data that starts with the prefix",
                Name = "Selected_Table_Prefix",
                Required = true
            };
            var fieldExplanation = new TextBlock()
            {
                Label = "followed by an integer will be grouped into a Table Row. Example field name: 'Line3_TravelExpense",
                Name = "Field_Explanation",
            };


            return PackControlsCrate(actionExplanation, upstreamDataChooser, fieldSelectPrefix, fieldExplanation);
        }


    }
}