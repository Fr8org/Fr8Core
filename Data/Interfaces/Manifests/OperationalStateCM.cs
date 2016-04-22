using Data.Constants;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.Manifests
{
    public partial class OperationalStateCM : Manifest
    {
        public ActivityCallStack CallStack { get; set; } = new ActivityCallStack();

        public List<HistoryElement> History { get; set; }
        public ActivityErrorCode? CurrentActivityErrorCode { get; set; }
        public string CurrentActivityErrorMessage { get; set; }
        public string CurrentClientActivityName { get; set; }
        public StackLocalData BypassData { get; set; }

        public ActivityResponseDTO CurrentActivityResponse { get; set; }

        public OperationalStateCM()
            : base(MT.OperationalStatus)
        {
            History = new List<HistoryElement>();
        }
    }
}
