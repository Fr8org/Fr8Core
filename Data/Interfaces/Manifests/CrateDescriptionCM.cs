using System.Collections.Generic;
using Data.Constants;
using Data.Crates;
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
    }
}
