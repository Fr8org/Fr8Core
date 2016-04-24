using System.Collections.Generic;
using System.Linq;
using Data.Crates;
using Data.Helpers;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

namespace TerminalBase.BaseClasses
{
    public class RuntimeCrateManager
    {
        public class FieldConfigurator
        {
            private readonly List<FieldDTO> _fields;
            private readonly string _label;
            private readonly CrateManifestType _manifestType;
            
            public FieldConfigurator(List<FieldDTO> fields, string label, CrateManifestType manifestType)
            {
                _fields = fields;
                _label = label;
                _manifestType = manifestType;
            }

            public FieldConfigurator AddFields(IEnumerable<FieldDTO> fields)
            {
                foreach (var fieldDto in fields)
                {
                    AddField(fieldDto);
                }

                return this;
            }

            public FieldConfigurator AddField(FieldDTO field)
            {
                field.SourceCrateLabel = _label;
                field.SourceCrateManifest = _manifestType;

                _fields.Add(field);

                return this;
            }

            public FieldConfigurator AddField(string name)
            {
                return AddField(new FieldDTO(name, AvailabilityType.RunTime)
                {
                    SourceCrateManifest = _manifestType,
                    SourceCrateLabel = _label
                });
            }
        }

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

        public void ClearAvailableCrates()
        {
            if (_runtimeAvailableData == null)
            {
                _runtimeAvailableData = _crateStorage.CrateContentsOfType<CrateDescriptionCM>(x => x.Label == RuntimeCrateDescriptionsCrateLabel).FirstOrDefault();
                _runtimeAvailableData?.CrateDescriptions?.Clear();
            }
        }

        public FieldConfigurator MarkAvailableAtRuntime<TManifest>(string label, bool suppressFieldDiscovery = false)
            where TManifest : Manifest
        {
            EnsureRuntimeDataCrate();

            var manifestType = ManifestDiscovery.Default.GetManifestType<TManifest>();
            var fields = new List<FieldDTO>();

            if (!suppressFieldDiscovery)
            {
                var members = Fr8ReflectionHelper.GetMembers(typeof (TManifest))
                    .Where(x => Fr8ReflectionHelper.IsPrimitiveType(x.MemberType))
                    .Where(x => Fr8ReflectionHelper.CheckAttributeOrTrue<ManifestFieldAttribute>(x, y => !y.IsHidden));

                foreach (var memberAccessor in members)
                {
                    fields.Add(new FieldDTO(memberAccessor.Name, AvailabilityType.RunTime)
                    {
                        SourceCrateLabel = label,
                        SourceCrateManifest = manifestType
                    });
                }
            }

            _runtimeAvailableData.AddOrUpdate(new CrateDescriptionDTO
            {
                Label = label,
                ManifestId = manifestType.Id,
                ManifestType = manifestType.Type,
                ProducedBy = _owner,
                Fields = fields
            });

            return new FieldConfigurator(fields, label, manifestType);
        }
    }
}
