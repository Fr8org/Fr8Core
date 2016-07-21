using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Manifests
{
    public partial class OperationalStateCM : Manifest
    {
        public ActivityCallStack CallStack { get; set; } = new ActivityCallStack();
        public List<HistoryElement> History { get; set; }

        //This property should not be persisted and should not be exposed outside the Hub
        [JsonIgnore]
        [ManifestField(IsHidden = true)]
        public StackLocalData BypassData { get; set; }

        public ActivityResponseDTO CurrentActivityResponse { get; set; }

        public OperationalStateCM()
            : base(MT.OperationalStatus)
        {
            History = new List<HistoryElement>();
        }
    }
}
