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

        public CrateDescriptionDTO AddIfMissing(CrateDescriptionDTO crateDescription)
        {
            var existingCrateDescription = CrateDescriptions.FirstOrDefault(x => x.Label == crateDescription.Label && x.ManifestId == crateDescription.ManifestId);

            if (existingCrateDescription != null)
            {
                return existingCrateDescription;
            }

            CrateDescriptions.Add(existingCrateDescription = crateDescription);

            return existingCrateDescription;
        }
    }
}
