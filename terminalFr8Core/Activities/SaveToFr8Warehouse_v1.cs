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
        public SaveToFr8Warehouse_v1() : base(false)
        {
        }

        protected override ActivityTemplateDTO MyTemplate { get; }
        public override Task Run()
        {
            // get the selected event from the drop down
            var crateChooser = GetControl<UpstreamCrateChooser>("UpstreamCrateChooser");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var manifestTypes = crateChooser.SelectedCrates.Select(c => c.ManifestType.Value);
                var curCrates = Payload.CratesOfType<Manifest>().Where(c => manifestTypes.Contains(c.ManifestType.Id.ToString(CultureInfo.InvariantCulture)));
                //get the process payload
                foreach (var curCrate in curCrates)
                {
                    var curManifest = curCrate.Content;
                    uow.MultiTenantObjectRepository.AddOrUpdate(CurrentUserId, curManifest);
                }

                uow.SaveChanges();
                Success();
            }

            return Task.FromResult(0);
        }

        public override async Task Initialize()
        {
            var mergedUpstreamRunTimeObjects = await MergeUpstreamFields("Available Run-Time Objects");
            FieldDTO[] upstreamLabels = mergedUpstreamRunTimeObjects.Content.
                Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();
            var configControls = new StandardConfigurationControlsCM();
            configControls.Controls.Add(ControlHelper.CreateUpstreamCrateChooser("UpstreamCrateChooser", "Store which crates?"));
            var curConfigurationControlsCrate = PackControls(configControls);
            //TODO let's leave this like that until Alex decides what to do
            var upstreamLabelsCrate = CrateManager.CreateDesignTimeFieldsCrate("AvailableUpstreamLabels", new FieldDTO[] { });
            //var upstreamLabelsCrate = Crate.CreateDesignTimeFieldsCrate("AvailableUpstreamLabels", upstreamLabels);
            var upstreamDescriptions = await GetCratesByDirection<ManifestDescriptionCM>(CrateDirection.Upstream);
            var upstreamRunTimeDescriptions = upstreamDescriptions.Where(c => c.Availability == AvailabilityType.RunTime);
            var fields = upstreamRunTimeDescriptions.Select(c => new FieldDTO(c.Content.Name, c.Content.Id));
            var upstreamManifestsCrate = CrateManager.CreateDesignTimeFieldsCrate("AvailableUpstreamManifests", fields.ToArray());
            Storage.Clear();
            Storage.Add(curConfigurationControlsCrate);
            Storage.Add(upstreamLabelsCrate);
            Storage.Add(upstreamManifestsCrate);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}