using System;
using System.Collections.Generic;
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

        public ConvertRelatedFieldsIntoTable_v1()
        {
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetProcessPayload(curActionDO, containerId);
            var storage = Crate.GetStorage(curPayloadDTO);
            //TODO: filter out crates by using UpstreamDataChooser

            var prefixValue = GetRowPrefix(curActionDO);
            
            
            var matchRegex = new Regex("^(" + prefixValue + ")[0-9]+");

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

        private Crate PackCrate_ConfigurationControls()
        {
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


            return PackControlsCrate(fieldSelectPrefix, fieldExplanation);
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
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
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

            if (hasControlsCrate)
            {
                return ConfigurationRequestType.Followup;
            }
            
            return ConfigurationRequestType.Initial;
            
        }
    }
}