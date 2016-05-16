using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hub.Managers;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.States;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace terminalFr8Core.Actions
{
    public class SaveToFr8Warehouse_v1 : BaseTerminalActivity
    {
        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var controls = GetConfigurationControls(activityDO);
            // get the selected event from the drop down
            var crateChooser = controls.FindByName<UpstreamCrateChooser>("UpstreamCrateChooser");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessPayload = await GetPayload(activityDO, containerId);
                var manifestTypes = crateChooser.SelectedCrates.Select(c => c.ManifestType.Value);

                var curCrates = CrateManager.FromDto(curProcessPayload.CrateStorage).CratesOfType<Manifest>().Where(c => manifestTypes.Contains(c.ManifestType.Id.ToString(CultureInfo.InvariantCulture)));

                //get the process payload
                foreach (var curCrate in curCrates)
                {
                    var curManifest = curCrate.Content;
                    uow.MultiTenantObjectRepository.AddOrUpdate(authTokenDO.UserID, curManifest);
                }

                uow.SaveChanges();
                return Success(curProcessPayload);
            }
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }
            
            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var mergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActivityDO, "Available Run-Time Objects");
            FieldDTO[] upstreamLabels = mergedUpstreamRunTimeObjects.Content.
                Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();

            var configControls = new StandardConfigurationControlsCM();
            configControls.Controls.Add(CreateUpstreamCrateChooser("UpstreamCrateChooser", "Store which crates?"));
            var curConfigurationControlsCrate = PackControls(configControls);
            
            //TODO let's leave this like that until Alex decides what to do
            var upstreamLabelsCrate = CrateManager.CreateDesignTimeFieldsCrate("AvailableUpstreamLabels", new FieldDTO[] { });
            //var upstreamLabelsCrate = Crate.CreateDesignTimeFieldsCrate("AvailableUpstreamLabels", upstreamLabels);


            var upstreamDescriptions = await GetCratesByDirection<ManifestDescriptionCM>(curActivityDO, CrateDirection.Upstream);
            var upstreamRunTimeDescriptions = upstreamDescriptions.Where(c => c.Availability == AvailabilityType.RunTime);
            var fields = upstreamRunTimeDescriptions.Select(c => new FieldDTO(c.Content.Name, c.Content.Id));
            var upstreamManifestsCrate = CrateManager.CreateDesignTimeFieldsCrate("AvailableUpstreamManifests", fields.ToArray());

            using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                crateStorage.Clear();
                crateStorage.Add(curConfigurationControlsCrate);
                crateStorage.Add(upstreamLabelsCrate);
                crateStorage.Add(upstreamManifestsCrate);
            }

            return curActivityDO;
        }
    }
}