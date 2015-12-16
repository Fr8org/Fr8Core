using Data.Constants;
using Data.Crates;
using Newtonsoft.Json;
using Utilities;
using System;

namespace Data.Interfaces.Manifests
{
    [CrateManifestType(Int32.MinValue, null)]
    public abstract class Manifest
    {
        private readonly CrateManifestType _manifestType;

        [JsonIgnore]
        public CrateManifestType ManifestType
        {
            get { return _manifestType; }
        }

        protected Manifest(MT manifestType)
            : this((int)manifestType, manifestType.GetEnumDisplayName())
        {
        }

        protected Manifest(int manifestType, string manifestName)
            :this(new CrateManifestType(manifestName, manifestType))
        {
        }

        protected Manifest(CrateManifestType manifestType)
        {
            _manifestType = manifestType;
        }
    }
}
