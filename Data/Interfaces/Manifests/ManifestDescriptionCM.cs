using System.Collections.Generic;
using Data.Crates;

namespace Data.Interfaces.Manifests
{
    public class ManifestDescriptionCM : Manifest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string SampleJSON { get; set; }
        public string Description { get; set; }
        public string RegisteredBy { get; set; }
        public ManifestDescriptionCM() : base(Constants.MT.ManifestDescription)
        {

        }
    }
}
