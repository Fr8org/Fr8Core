using System.Linq;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

namespace TerminalBase.BaseClasses
{
    public class RuntimeCrateManager
    {
        private readonly ICrateStorage _crateStorage;
        private readonly string _owner;
        public const string RuntimeCrateDescriptionsCrateLabel = "Runtime Available Crates";
        private CrateDescriptionCM _runtimeAvailableData;

        public RuntimeCrateManager(ICrateStorage crateStorage, string owner)
        {
            _crateStorage = crateStorage;
            _owner = owner;
        }

        private void EnsureRuntimeDataCrate()
        {
            if (_runtimeAvailableData == null)
            {
                _runtimeAvailableData = _crateStorage.CrateContentsOfType<CrateDescriptionCM>(x => x.Label == RuntimeCrateDescriptionsCrateLabel).FirstOrDefault();
                
                if (_runtimeAvailableData == null)
                {
                    _runtimeAvailableData = new CrateDescriptionCM();
                    _crateStorage.Add(Crate.FromContent(RuntimeCrateDescriptionsCrateLabel, _runtimeAvailableData, AvailabilityType.RunTime));
                }
            }
        }

        public void MarkAvailableAtRuntime<TManifest>(string label)
            where TManifest : Manifest
        {
            EnsureRuntimeDataCrate();

            var manifestType = ManifestDiscovery.Default.GetManifestType<TManifest>();

            _runtimeAvailableData.AddIfNotExists(new CrateDescriptionDTO
            {
                Label = label,
                ManifestId = manifestType.Id,
                ManifestType = manifestType.Type,
                ProducedBy = _owner,
            });
        }
    }
}
