using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class ManifestDescriptionCM : Manifest
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string SampleJSON { get; set; }

        public string Description { get; set; }

        public string RegisteredBy { get; set; }

        public ManifestDescriptionCM()
            : base(MT.ManifestDescription)
        {

        }
    }
}
