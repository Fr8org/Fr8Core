using Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.Manifests
{
    public class OperationalStatusCM : Manifest
    {
        public int LoopIndex { get; set; }
        public bool Break { get; set; }
        public int LoopLevel { get; set; }

        public OperationalStatusCM()
            : base(MT.OperationalStatus)
        {
        }

        public void IncreaseLoopIndex()
        {
            ++LoopIndex;
        }

        public void BreakLoop()
        {
            Break = true;
        }
    }
}
