using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

using Hub.Managers;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Actions
{
    public class MapFields_v1 : BaseTerminalAction
    {
        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curControlsMS = Crate.GetStorage(actionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (curControlsMS == null)
            {
                throw new ApplicationException("No controls crate found.");
            }

            var curMappingControl = curControlsMS.Controls.FirstOrDefault(x => x.Name == "Selected_Mapping");

            if (curMappingControl == null || string.IsNullOrEmpty(curMappingControl.Value))
            {
                throw new ApplicationException("No Selected_Mapping control found.");
            }

            var mappedFields = JsonConvert.DeserializeObject<List<FieldDTO>>(curMappingControl.Value);
            mappedFields = mappedFields.Where(x => x.Key != null && x.Value != null).ToList();

            var processPayload = await GetProcessPayload(actionDO, containerId);

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("MappedFields", new StandardPayloadDataCM(mappedFields)));
            }
            return processPayload;
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public override async Task<ActionDO> Configure(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(actionDO, ConfigurationEvaluator, authTokenDO);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private void AddMappingPane(CrateStorage storage)
        {
            var mappingPane = new MappingPane()
            {
                Label = "Configure Mapping",
                Name = "Selected_Mapping",
                Required = true
            };

            AddControl(storage, mappingPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //Filter the upstream fields by Availability flag as this action takes the run time data (left DDLBs) to the fidles (right DDLBs)
            var curUpstreamFields =
                (await GetDesignTimeFields(curActionDO, CrateDirection.Upstream))
                .Fields.Where(field => field.Availability != AvailabilityType.Configuration)
                .ToArray();

            //Get all the downstream fields to be mapped (right DDLBs)
            var curDownstreamFields =
                (await GetDesignTimeFields(curActionDO, CrateDirection.Downstream))
                .Fields
                .ToArray();

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            var downstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Downstream Terminal-Provided Fields", curDownstreamFields);
            var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
                {
                    AddErrorTextBlock(updater.CrateStorage);
                }
                else
                {
                    AddMappingPane(updater.CrateStorage);
                }

                updater.CrateStorage.Add(downstreamFieldsCrate);
                updater.CrateStorage.Add(upstreamFieldsCrate);
            }

            return curActionDO;
        }

        private void AddErrorTextBlock(CrateStorage storage)
        {
            var textBlock = new TextBlock()
            {
                Name = "MapFieldsErrorMessage",
                Label = "Error",
                Value = "This Action works by mapping upstream data (from the left) to downstream fields (on the right)",
                CssClass = "well well-lg"
            };

            AddControl(storage, textBlock);
        }

        /// <summary>
        /// Check if initial configuration was requested.
        /// </summary>
        private bool CheckIsInitialConfiguration(ActionDO curAction)
        {
            CrateStorage storage;

            // Check nullability for CrateStorage and Crates array length.
            if (curAction.CrateStorage == null || (storage = Crate.GetStorage(curAction.CrateStorage)).Count == 0)
            {
                return true;
            }

            var upStreamFields = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Terminal-Provided Fields").FirstOrDefault();
            var downStreamFields = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Downstream Terminal-Provided Fields").FirstOrDefault();

            if (upStreamFields == null
                || upStreamFields.Fields == null
                || upStreamFields.Fields.Count == 0
                || downStreamFields == null
                || downStreamFields.Fields == null
                || downStreamFields.Fields.Count == 0)
            {
                return true;
            }

            // If all rules are passed, then it is not an initial configuration request.
            return false;
        }
        /// <summary>
        /// ConfigurationEvaluator always returns Initial,
        /// since Initial and FollowUp phases are the same for current action.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (CheckIsInitialConfiguration(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }
    }
}