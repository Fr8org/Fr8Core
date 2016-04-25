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

        public CrateDescriptionDTO AddOrUpdate(CrateDescriptionDTO crateDescription)
        {
            for (int i = 0; i < CrateDescriptions.Count; i ++)
            {
                var x = CrateDescriptions[i];

                if (x.Label == crateDescription.Label && x.ManifestId == crateDescription.ManifestId)
                {
                    CrateDescriptions[i] = crateDescription;
                    return crateDescription;
                }
            }

            CrateDescriptions.Add(crateDescription);

            return crateDescription;
        }
    }
}
