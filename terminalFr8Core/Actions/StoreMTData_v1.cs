using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Control;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Repositories;
using System.Reflection;

namespace terminalFr8Core.Actions
{
    public class StoreMTData_v1 : BaseTerminalAction
    {
        private class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownList Events { get; set; }

            public ActionUi(string label, string manifestType)
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(Events = new DropDownList()
                {
                    Label = "Save Which Data Types?",
                    Name = "Save Object Name",
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        Label = label,
                        ManifestType = manifestType
                    }
                });

            }
        }
        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var controls = Crate.GetStorage(actionDO)
               .CrateContentsOfType<StandardConfigurationControlsCM>()
               .SingleOrDefault();

            // get the selected event from the drop down
            var specificEvent = (DropDownList)controls.Controls[0];

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                //get the process payload
                var curProcessPayload = await GetPayload(actionDO, containerId);
                var curCrates = Crate.FromDto(curProcessPayload.CrateStorage).CratesOfType<Manifest>().Where(d => d.Label == specificEvent.selectedKey);

                foreach (var curCrate in curCrates)
                {
                    var curManifest = (Manifest)curCrate.Content;

                    // Use reflection to call the generic method
                    MethodInfo method = typeof(MultiTenantObjectRepository).GetMethod("AddOrUpdate");
                    MethodInfo addOrUpdate = method.MakeGenericMethod(curManifest.GetType());
                    addOrUpdate.Invoke(uow.MultiTenantObjectRepository, new object[] { uow, authTokenDO.UserID, curManifest, null });
                }

                return Success(curProcessPayload);
            }
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }
            var storage = Crate.GetStorage(curActionDO);

            var hasAvailableRunTimeObjectsCrate = storage
                .CratesOfType<StandardDesignTimeFieldsCM>(c => c.Label == "Available Run-Time Objects").FirstOrDefault() != null;

            var hasSelectedObjectTypeCrate = storage
                .CratesOfType<StandardDesignTimeFieldsCM>(c => c.Label == "SelectedObjectTypes").FirstOrDefault() != null;

            var hasConfigurationControlsCrate = storage
                .CratesOfType<StandardConfigurationControlsCM>(c => c.Label == "Configuration_Controls").FirstOrDefault() != null;


            if (hasAvailableRunTimeObjectsCrate && hasSelectedObjectTypeCrate && hasConfigurationControlsCrate)
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {

            var curMergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActionDO, "Available Run-Time Objects");

            var curConfigurationControlsCrate = PackControls(
                new ActionUi(curMergedUpstreamRunTimeObjects.Label, curMergedUpstreamRunTimeObjects.ManifestType.Type));

            FieldDTO[] curSelectedFields = curMergedUpstreamRunTimeObjects.Content.
                Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();

            var curSelectedObjectType = Crate.CreateDesignTimeFieldsCrate("SelectedObjectTypes", curSelectedFields);

            using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(curMergedUpstreamRunTimeObjects);
                updater.CrateStorage.Add(curConfigurationControlsCrate);
                updater.CrateStorage.Add(curSelectedObjectType);
            }

            return curActionDO;
        }
    }
}