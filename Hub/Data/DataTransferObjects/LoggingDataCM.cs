using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Newtonsoft.Json.Linq;

namespace Fr8Data.DataTransferObjects
{

    public class LoggingDataCM : Manifest
    {
        public string ObjectId { get; set; }

        public string Fr8UserId { get; set; }

        public string Data { get; set; }

        public string PrimaryCategory { get; set; }

        public string SecondaryCategory { get; set; }

        public string Activity { get; set; }

        public LoggingDataCM()
            : base(MT.LoggingData)
        {
        }
    }
    
   
}
