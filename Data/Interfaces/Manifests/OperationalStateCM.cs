using Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.Manifests
{
    public class OperationalStateCM : Manifest
    {
        public class LoopStatus
        {
            public string Id { get; set; }
            public int Index { get; set; }
            public bool BreakSignalReceived { get; set; }
            public int Level { get; set; }
        }

        public class ActionStateMatch
        {
            public string Id { get; set; }
            public ActionState State { get; set; }
        }

        public List<LoopStatus> Loops { get; set; }

        public List<ActionStateMatch> States { get; set; }

        public Guid PausedRouteNodeId { get; set; }
        

        public OperationalStateCM()
            : base(MT.OperationalStatus)
        {
            Loops = new List<LoopStatus>();
        }

    }
}
