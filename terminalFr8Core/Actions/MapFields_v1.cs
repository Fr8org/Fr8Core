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
                .Fields.Where(field => field.Availability != AvailabilityType.Configuration)
                .ToArray();

            if (!(NeedsConfiguration(curActionDO, curUpstreamFields, curDownstreamFields)))
                return curActionDO;

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
                    AddInitialTextBlock(updater.CrateStorage);
                    AddMappingPane(updater.CrateStorage);
                }

                updater.CrateStorage.Add(downstreamFieldsCrate);
                updater.CrateStorage.Add(upstreamFieldsCrate);
            }

            return curActionDO;
        }

        private void AddInitialTextBlock(CrateStorage storage)
        {
            var textBlock = new TextBlock()
            {
                Name = "InfoBlock",
                Value = "When this route is executed, the values found in the fields on the left will be used for the fields on the right",
                CssClass = "well well-lg"
            };

            AddControl(storage, textBlock);
        }

        private void AddErrorTextBlock(CrateStorage storage)
        {
            var textBlock = new TextBlock()
            {
                Name = "MapFieldsErrorMessage",
                Value = "In order to work this Action needs upstream and downstream Actions configured",
                CssClass = "well well-lg"
            };

            AddControl(storage, textBlock);
        }

        /// <summary>
        /// Check if initial configuration was requested.
        /// </summary>
        private bool NeedsConfiguration(ActionDO curAction, FieldDTO[] curUpstreamFields, FieldDTO[] curDownstreamFields)
        {
            CrateStorage storage = storage = Crate.GetStorage(curAction.CrateStorage);

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
            else
            {
                // true if current up/downstream fields don't match saved up/downstream fields
                bool upstreamMatch = upStreamFields.Fields.Where(a => curUpstreamFields.Any(b => b.Key == a.Key)).Count() == upStreamFields.Fields.Count;
                bool downstreamMatch = downStreamFields.Fields.Where(a => curDownstreamFields.Any(b => b.Key == a.Key)).Count() == downStreamFields.Fields.Count;

                return !(upstreamMatch & downstreamMatch);
            }
        }


        /// <summary>
        /// ConfigurationEvaluator always returns Initial,
        /// since Initial and FollowUp phases are the same for current action.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;

        }
    }
}