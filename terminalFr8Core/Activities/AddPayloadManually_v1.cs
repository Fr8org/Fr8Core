using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
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
using Data.Control;
using Data.States;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class AddPayloadManually_v1 : BaseTerminalActivity
    {

        private const string RunTimeCrateLabel = "ManuallyAddedPayload";

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            var controlsMS = CrateManager.GetStorage(curActivityDO.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return Error(payloadCrates, "Could not find ControlsConfiguration crate.");
            }

            var fieldListControl = controlsMS.Controls
                .SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                return Error(payloadCrates, "Could not find FieldListControl.");
            }

            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);

            using (var crateStorage = CrateManager.UpdateStorage(() => payloadCrates.CrateStorage))
            {
                crateStorage.Add(Data.Crates.Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(userDefinedPayload)));
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

            return Success(payloadCrates);
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                var availableRunTimeCrates = Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.StandardPayloadData.GetEnumDisplayName(),
                        Label = RunTimeCrateLabel,
                        ManifestId = (int)MT.StandardPayloadData,
                        ProducedBy = "AddPayloadManually_v1"
                    }), AvailabilityType.RunTime);

                crateStorage.Add(availableRunTimeCrates);
            }

            return Task.FromResult(curActivityDO);
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                throw new ApplicationException("Could not find ControlsConfiguration crate.");
            }

            var fieldListControl = controlsMS.Controls.SingleOrDefault(x => x.Type == ControlTypes.FieldList);

            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            if (fieldListControl.Value != null)
            {
                var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);
                userDefinedPayload.ForEach(x =>
                {
                    x.Value = x.Key;
                    x.Availability = Data.States.AvailabilityType.RunTime;
                });

                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.RemoveByLabel(RunTimeCrateLabel);
                    var crate = Data.Crates.Crate.FromContent(RunTimeCrateLabel, new FieldDescriptionsCM() { Fields = userDefinedPayload });
                    crate.Availability = Data.States.AvailabilityType.RunTime;
                    crateStorage.Add(crate);
                }

            }

            return Task.FromResult(curActivityDO);
        }

        private Crate CreateControlsCrate()
        {
            var fieldFilterPane = new FieldList
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Required = true,
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };

            return PackControlsCrate(fieldFilterPane);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}