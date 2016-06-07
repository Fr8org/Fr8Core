using System.Collections.Generic;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace fr8.Infrastructure.Data.Manifests
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
