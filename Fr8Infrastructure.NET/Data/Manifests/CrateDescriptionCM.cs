using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class CrateDescriptionCM : Manifest
    {
        [JsonProperty("crateDescriptions")]
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
