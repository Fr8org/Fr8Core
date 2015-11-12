using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Data.Entities;
using StructureMap;
using Hub.Managers;

namespace terminalFr8Core.Actions
{
    public class AddPayloadManually_v1 : BaseTerminalAction
    {
        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            var processPayload = await GetProcessPayload(containerId);

            var controlsMS = Crate.GetStorage(curActionDO.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var fieldListControl = controlsMS.Controls
                .SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);

            using (var updater = Crate.UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("ManuallyAddedPayload", new StandardPayloadDataCM(userDefinedPayload)));
            }
//
//            var cratePayload = Crate.Create(
//                "Manual Payload Data",
//                JsonConvert.SerializeObject(userDefinedPayload),
//                CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
//                CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID
//                );
//
//            processPayload.UpdateCrateStorageDTO(new List<CrateDTO>() { cratePayload });

            return processPayload;
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO = null)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
            }

            return Task.FromResult(curActionDO);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var fieldListControl = controlsMS.Controls.SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);
            userDefinedPayload.ForEach(x => x.Value = x.Key);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.RemoveByLabel("ManuallyAddedPayload");
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("ManuallyAddedPayload", new StandardDesignTimeFieldsCM() { Fields = userDefinedPayload }));
            }

            return Task.FromResult(curActionDO);
        }

        private Crate CreateControlsCrate()
        {
            var fieldFilterPane = new FieldListControlDefinitionDTO
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}