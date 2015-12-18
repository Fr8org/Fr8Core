using System;
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
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            var storage = Crate.GetStorage(curPayloadDTO);

            var designTimeStorage = Crate.GetStorage(curActionDO);
            var designTimeControls = designTimeStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
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
            //not sure what to do with this
            if (upstreamDataChooser.SelectedFieldType != null)
            {
                //filteredCrates = filteredCrates.Where(s => s.?? == upstreamDataChooser.SelectedFieldType);
            }

            var prefixValue = GetRowPrefix(curActionDO);
            
            
            var matchRegex = new Regex("^(" + prefixValue + ")[0-9]+");
            
            //TODO get data based on what????
            var rows = storage.CratesOfType<StandardPayloadDataCM>() //get payload data
                .Select(p => p.Content)
                .SelectMany(p => p.PayloadObjects)
                .Select(po => po.PayloadObject)
                .SelectMany(po => po)
                .Where(f => matchRegex.IsMatch(f.Key))
                .Select(f => new
                {
                    lineNumber = Int32.Parse(Regex.Match(f.Key, FirstIntegerRegexPattern).Value),
                    field = f
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

            return curPayloadDTO;
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

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authToken)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authToken);
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

        /// <summary>
        /// Looks for first Create with Id == "Standard Design-Time" among all upcoming Actions.
        /// </summary>
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            
            if (curActionDO.Id != Guid.Empty)
            {
                //this conversion from actiondto to Action should be moved back to the controller edge
                /*var curUpstreamFields =
                    (await GetDesignTimeFields(curActionDO, CrateDirection.Upstream))
                    .Fields
                    .ToArray();*/

                //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
                //var queryFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", curUpstreamFields);

                //build a controls crate to render the pane
                var configurationControlsCrate = PackCrate_ConfigurationControls();

                using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
                {
                    updater.CrateStorage = AssembleCrateStorage(/*queryFieldsCrate, */configurationControlsCrate);
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
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().Single();
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

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDataPackageDO)
        {
            if (Crate.IsStorageEmpty(curActionDataPackageDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var storage = Crate.GetStorage(curActionDataPackageDO);
            var hasControlsCrate = storage.CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls").Any();

            var hasManifestTypeList = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Manifest Type List").Any();

            if (hasControlsCrate && hasManifestTypeList)
            {
                return ConfigurationRequestType.Followup;
            }
            
            return ConfigurationRequestType.Initial;
            
        }
    }
}