using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Hub.Services;
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

namespace terminalFr8Core.Actions
{
    public class QueryMTDatabase_v1 : BaseTerminalAction
    {
        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            var objectList = GetObjects();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("Available_Objects", objectList.ToArray()));
            }

            return Task.FromResult(curActionDO);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
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

            if (fieldListControl.Value != null)
            {
                var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);
                userDefinedPayload.ForEach(x => x.Value = x.Key);

                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.RemoveByLabel("ManuallyAddedPayload");
                    updater.CrateStorage.Add(Data.Crates.Crate.FromContent("ManuallyAddedPayload", new StandardDesignTimeFieldsCM() { Fields = userDefinedPayload }));
                }
            }

            return Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActionDO, containerId);

            var controlsMS = Crate.GetStorage(curActionDO.CrateStorage).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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

            using (var updater = Crate.UpdateStorage(() => payloadCrates.CrateStorage))
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

            return Success(payloadCrates);
        }

        private IEnumerable<FieldDTO> GetObjects()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.MTObjectRepository.GetAll().Select(c => new FieldDTO(c.Name, c.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private Crate CreateControlsCrate()
        {
            var objectListDropdown = new DropDownList
            {
                Label = "Object List",
                Name = "Available_Objects",
                Value = null,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Objects",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
                
            };

            var fieldFilterPane = new FilterPane()
            {
                Label = "Find all Fields where:",
                Name = "Selected_Filter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Fields",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            return PackControlsCrate(objectListDropdown, fieldFilterPane);
        }
    }
}