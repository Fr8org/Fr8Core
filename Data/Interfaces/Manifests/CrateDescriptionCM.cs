using System.Collections.Generic;
using System.Linq;
using Data.Constants;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class CrateDescriptionCM : Manifest
    {
        public List<CrateDescriptionDTO> CrateDescriptions { get; set; }
        public CrateDescriptionCM() : base(MT.CrateDescription)
        {
            CrateDescriptions = new List<CrateDescriptionDTO>();
        }

        public CrateDescriptionCM(IEnumerable<CrateDescriptionDTO> crateDescriptions) : this()
        {
            CrateDescriptions.AddRange(crateDescriptions);
        }

        public CrateDescriptionCM(params CrateDescriptionDTO[] crateDescriptions) : this()
        {
            CrateDescriptions.AddRange(crateDescriptions);
        }

        //public CrateDescriptionCM(int manifestId, string manifestType, string label) : this()
        //{
        //    CrateDescriptions.Add(new CrateDescriptionDTO { ManifestType = manifestType, Label = label, ManifestId = manifestId});
        //}

        public void AddIfNotExists(CrateDescriptionDTO crateDescription)
        {
            if (CrateDescriptions.Any(x => x.Label == crateDescription.Label && x.ManifestId == crateDescription.ManifestId))
            {
                return;
            }

            CrateDescriptions.Add(crateDescription);
        }
    }
}
