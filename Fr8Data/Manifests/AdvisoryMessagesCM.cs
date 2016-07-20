using System.Collections.Generic;
using Fr8Data.Constants;
using Fr8Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8Data.Manifests
{
    public class AdvisoryMessagesCM : Manifest
    {
        public AdvisoryMessagesCM() : base(MT.AdvisoryMessages)
        {
            Advisories = new List<AdvisoryMessageDTO>();
        }

        [JsonProperty("advisories")]
        public List<AdvisoryMessageDTO> Advisories { get; set; }
    }
}
