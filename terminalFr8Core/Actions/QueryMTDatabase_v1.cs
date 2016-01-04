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
            

            return Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActionDO, containerId);
            return Success(payloadCrates);
        }

        private IEnumerable<FieldDTO> GetFieldsByObjectId(string objectId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.MTObjectRepository.GetAll().Select(c => new FieldDTO(c.Name, c.Id.ToString(CultureInfo.InvariantCulture)));
            }
        }

        private Guid GetCurrentFr8UserId(ActionDO curActionDO)
        {
            return Guid.NewGuid();
            /*
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRepository.GetQuery().Where(a => a.Id == curActionDO.Id).Select(a => a.);
            }*/
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
                Events = new List<ControlEvent>{ ControlEvent.RequestConfig },
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