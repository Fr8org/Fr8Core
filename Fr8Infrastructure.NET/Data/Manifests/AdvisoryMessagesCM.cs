using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Manifests
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
