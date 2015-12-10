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
        public class LoopStatus
        {
            public string LoopId { get; set; }
            public int LoopIndex { get; set; }
            public bool Break { get; set; }
            public int LoopLevel { get; set; }
            public void IncreaseLoopIndex()
            {
                ++LoopIndex;
            }

            public void BreakLoop()
            {
                Break = true;
            }
        }

        public List<LoopStatus> Loops { get; set; } 
        

        public OperationalStatusCM()
            : base(MT.OperationalStatus)
        {
            Loops = new List<LoopStatus>();
        }

        public LoopStatus GetLoopById(string loopId)
        {
            return Loops.FirstOrDefault(l => l.LoopId == loopId);
        }

        public void CreateLoop(string loopId)
        {
            var loopLevel = Loops.Count();
            Loops.Add(new LoopStatus
            {
                Break = false,
                LoopId = loopId,
                LoopIndex = 0,
                LoopLevel = loopLevel
            });
        }

        public void RemoveLoop(string loopId)
        {
            var loop = GetLoopById(loopId);
            Loops.Remove(loop);
        }


    }
}
