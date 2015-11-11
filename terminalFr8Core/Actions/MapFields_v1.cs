
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using Data.Crates;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
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
        public async Task<PayloadDTO> Run(ActionDO actionDO, int containerId, AuthorizationTokenDO authTokenDO=null)
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

            var processPayload = await GetProcessPayload(containerId);

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("MappedFields", new StandardPayloadDataCM(mappedFields)));
//                var actionPayloadCrates = new List<CrateDTO>()
//                {
//                    Crate.Create("MappedFields",
//                        JsonConvert.SerializeObject(mappedFields),
//                        CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
//                        CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID)
//                };
//
//                processPayload.UpdateCrateStorageDTO(actionPayloadCrates);
            }
            return processPayload;
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public async Task<ActionDO> Configure(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(actionDO, ConfigurationEvaluator, authTokenDO);
        }

//        private void FillCrateConfigureList(IEnumerable<ActionDO> actions,
//            List<MappingFieldConfigurationDTO> crateConfigList)
//        {
//            foreach (var curAction in actions)
//            {
//                var curCrateStorage = curAction.CrateStorageDTO();
//                foreach (var curCrate in curCrateStorage.CrateDTO)
//                {
//                    crateConfigList.Add(new MappingFieldConfigurationDTO()
//                    {
//                        Id = curCrate.Id,
//                        Label = curCrate.Label
//                    });
//                }
//            }
//        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private Crate CreateStandardConfigurationControls()
        {
            var fieldFilterPane = new MappingPaneControlDefinitionDTO()
            {
                Label = "Configure Mapping",
                Name = "Selected_Mapping",
                Required = true
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            Crate getErrorMessageCrate = null;

            var curUpstreamFields =
                (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Upstream))
                .Fields
                .ToArray();

            var curDownstreamFields =
                (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Downstream))
                .Fields
                .ToArray();

            if (curUpstreamFields.Length == 0 || curDownstreamFields.Length == 0)
            {
                getErrorMessageCrate = GetTextBoxControlForDisplayingError("MapFieldsErrorMessage",
                         "This action couldn't find either source fields or target fields (or both). " +
                        "Try configuring some Actions first, then try this page again.");
                curActionDO.currentView = "MapFieldsErrorMessage";
            }

            //Pack the merged fields into 2 new crates that can be used to populate the dropdowns in the MapFields UI
            var downstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Downstream Terminal-Provided Fields", curDownstreamFields);
            var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);

            var curConfigurationControlsCrate = CreateStandardConfigurationControls();


            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(downstreamFieldsCrate);
                updater.CrateStorage.Add(upstreamFieldsCrate);
                updater.CrateStorage.Add(curConfigurationControlsCrate);

                if (getErrorMessageCrate != null)
            {
                    updater.CrateStorage.Add(getErrorMessageCrate);
                }
            }

            return curActionDO;
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

//            // Check nullability of Upstream and Downstream crates.
//            var upStreamFieldsCrate = curAction.CrateStorage.CrateDTO.FirstOrDefault(
//                x => x.Label == "Upstream Plugin-Provided Fields"
//                    && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);
//
//            var downStreamFieldsCrate = curAction.CrateStorage.CrateDTO.FirstOrDefault(
//                x => x.Label == "Downstream Plugin-Provided Fields"
//                    && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);
//
//            if (upStreamFieldsCrate == null
//                || string.IsNullOrEmpty(upStreamFieldsCrate.Contents)
//                || downStreamFieldsCrate == null
//                || string.IsNullOrEmpty(downStreamFieldsCrate.Contents))
//            {
//                return true;
//            }
//
//            // Check if Upstream and Downstream ManifestSchemas contain empty set of fields.
//            var upStreamFields = JsonConvert
//                .DeserializeObject<StandardDesignTimeFieldsCM>(upStreamFieldsCrate.Contents);
//
//            var downStreamFields = JsonConvert
//                .DeserializeObject<StandardDesignTimeFieldsCM>(downStreamFieldsCrate.Contents);

            if (upStreamFields.Fields == null
                || upStreamFields.Fields.Count == 0
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
        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
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