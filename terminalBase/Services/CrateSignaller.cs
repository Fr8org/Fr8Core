using System;
using System.Collections.Generic;
using System.Linq;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Helpers;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace TerminalBase.Services
{
    public class CrateSignaller
    {
        public class FieldConfigurator
        {
            private readonly List<FieldDTO> _fields;

            private readonly string _label;

            private readonly CrateManifestType _manifestType;

            private readonly Guid _sourceActivityId;

            public FieldConfigurator(List<FieldDTO> fields, string label, CrateManifestType manifestType, Guid sourceActivityId)
            {
                _fields = fields;
                _label = label;
                _manifestType = manifestType;
                _sourceActivityId = sourceActivityId;
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
                field.SourceActivityId = _sourceActivityId.ToString();
                _fields.Add(field);

                return this;
            }

            public FieldConfigurator AddField(string name)
            {
                return AddField(new FieldDTO(name, AvailabilityType.RunTime)
                {
                    SourceCrateManifest = _manifestType,
                    SourceCrateLabel = _label,
                    SourceActivityId = _sourceActivityId.ToString()
                });
            }
        }

        private readonly ICrateStorage _crateStorage;

        private readonly string _owner;

        private readonly Guid _sourceActivityId;

        public const string RuntimeCrateDescriptionsCrateLabel = "Runtime Available Crates";

        private CrateDescriptionCM _runtimeAvailableData;
        
        public CrateSignaller(ICrateStorage crateStorage, string owner, Guid sourceActivityId)
        {
            _crateStorage = crateStorage;
            _owner = owner;
            _sourceActivityId = sourceActivityId;
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

            return new FieldConfigurator(fields, label, manifestType, _sourceActivityId);
        }
    }
}
