using Data.Constants;
using Utilities;

namespace Data.Interfaces.Manifests
{
    public abstract class Manifest
    {
        public MT ManifestType { get; private set; }
      
        public int ManifestId
        {
            get { return (int)ManifestType; }
        }

        public string ManifestName
        {
            get
            {
                if (ManifestType == 0)
                {
                    return null;
                }

                return ManifestType.GetEnumDisplayName();
            }
        }

        protected Manifest(MT manifestType)
        {
            ManifestType = manifestType;
        }
    }
}
