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
using terminalFr8Core.Interfaces;

namespace terminalFr8Core.Actions
{

    public class ConvertRelatedFieldsIntoTable_v1 : BaseTerminalAction
    {
        private const string FirstIntegerRegexPattern = "\\d+";

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authToken)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authToken);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDataPackageDO)
        {
            if (Crate.IsStorageEmpty(curActionDataPackageDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var storage = Crate.GetStorage(curActionDataPackageDO);
            var hasControlsCrate = GetConfigurationControls(curActionDataPackageDO) != null;

            var hasManifestTypeList = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Manifest Type List").Any();

            if (hasControlsCrate && hasManifestTypeList)
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;

        }

        /// <summary>
        /// Looks for first Create with Id == "Standard Design-Time" among all upcoming Actions.
        /// </summary>
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                //build a controls crate to render the pane
                var configurationControlsCrate = PackCrate_ConfigurationControls();

                using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
                {
                    updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                    updater.CrateStorage.Add(await GetUpstreamManifestTypes(curActionDO));
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }

            return curActionDO;
        }


        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = GetConfigurationControls(curActionDO);
            var upstreamDataChooser = (UpstreamDataChooser)controlsMS.Controls.Single(x => x.Type == ControlTypes.UpstreamDataChooser && x.Name == "Upstream_data_chooser");
            if (upstreamDataChooser.SelectedManifest != null)
            {
                var labelList = await GetLabelsByManifestType(curActionDO, upstreamDataChooser.SelectedManifest);

                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.RemoveByLabel("Upstream Crate Label List");
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Upstream Crate Label List", new StandardDesignTimeFieldsCM() { Fields = labelList }));
                }
            }

            return curActionDO;
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            var storage = Crate.GetStorage(curPayloadDTO);

            var designTimeControls = GetConfigurationControls(curActionDO);
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

            var fieldList = FindFieldsOfCrates(filteredCrates);

            
            if (upstreamDataChooser.SelectedFieldType != null)
            {
                //not quite sure what to do with this
                fieldList = fieldList.Where(s => s.Tags == upstreamDataChooser.SelectedFieldType);
            }


            var prefixValue = GetRowPrefix(curActionDO);
            if (prefixValue == null)
            {
                return Error(curPayloadDTO/*, "This action can't run without a selected column prefix"*/);
            }
            var matchRegex = new Regex("^(" + prefixValue + ")[0-9]+");
            
            var rows = fieldList
                .Where(f => matchRegex.IsMatch(f.Key))
                .Select(f => new
                {
                    lineNumber = Int32.Parse(Regex.Match(f.Key, FirstIntegerRegexPattern).Value),
                    field = new FieldDTO(f.Key.Substring(f.Key.IndexOf(Regex.Match(f.Key, FirstIntegerRegexPattern).Value, StringComparison.Ordinal)+1), f.Value)
                })
                .GroupBy(r => r.lineNumber)
                .Select(r => new TableRowDTO
                {
                    Row = r.Select(f => new TableCellDTO
                    {
                        Cell = f.field
                    }).ToList()
                });

            var tableDataCrate = Crate.CreateStandardTableDataCrate("AssembledTableData", false, rows.ToArray());
            using (var updater = Crate.UpdateStorage(curPayloadDTO))
            {
                updater.CrateStorage.Add(tableDataCrate);
            }

            return Success(curPayloadDTO);
        }

        private IEnumerable<FieldDTO> FindFieldsOfCrates(IEnumerable<Crate> crates)
        {
            var fields = new List<FieldDTO>();

            foreach (var crate in crates)
            {
                //let's pass unknown manifests for now
                if (!crate.IsKnownManifest)
                {
                    continue;
                }

                fields.AddRange(FindFieldsRecursive(crate.Get()));                
            }

            return fields;
        }

        private static IEnumerable<FieldDTO> FindFieldsRecursive(Object obj)
        {
            var fields = new List<FieldDTO>();
            if (obj is IEnumerable)
            {
                
                var objList = obj as IEnumerable;
                foreach (var element in objList)
                {
                    fields.AddRange(FindFieldsRecursive(element));
                }
                return fields;
            }

            var objType = obj.GetType();
            bool isPrimitiveType = objType.IsPrimitive || objType.IsValueType || (objType == typeof(string));

            if (!isPrimitiveType)
            {
                var field = obj as FieldDTO;
                if (field != null)
                {
                    return new List<FieldDTO> {field};
                }

                var objProperties = objType.GetProperties();
                var objFields = objType.GetFields();
                foreach (var prop in objProperties)
                {
                    fields.AddRange(FindFieldsRecursive(prop.GetValue(obj)));
                }

                foreach (var prop in objFields)
                {
                    fields.AddRange(FindFieldsRecursive(prop.GetValue(obj)));
                }
            }

            return fields;
        }

        private string GetRowPrefix(ActionDO curActionDO)
        {
            var actionStorage = Crate.GetStorage(curActionDO);
            var confControls = actionStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls")
                .SelectMany(c => c.Controls);

            var prefixValue = confControls.Single(c => c.Name == "Selected_Table_Prefix").Value;
            return prefixValue;
        }

        

        private async Task<Crate> GetUpstreamManifestTypes(ActionDO curActionDO)
        {
            var upstreamCrates = await GetCratesByDirection(curActionDO, CrateDirection.Upstream);
            var manifestTypeOptions = upstreamCrates.GroupBy(c => c.ManifestType).Select(c => new FieldDTO(c.Key.Type, c.Key.Type));
            var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Manifest Type List", manifestTypeOptions.ToArray());
            return queryFieldsCrate;
        }

        private async Task<List<FieldDTO>> GetLabelsByManifestType(ActionDO curActionDO, string manifestType)
        {
            var upstreamCrates = await GetCratesByDirection(curActionDO, CrateDirection.Upstream);
            return upstreamCrates
                    .Where(c => c.ManifestType.Type == manifestType)
                    .GroupBy(c => c.Label)
                    .Select(c => new FieldDTO(c.Key, c.Key)).ToList();
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
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };
            var fieldSelectPrefix = new TextBox()
            {
                Label = "All field data that starts with the prefix",
                Name = "Selected_Table_Prefix",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
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