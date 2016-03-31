using Data.Constants;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace Data.Interfaces.Manifests
{
    public class OperationalStateCM : Manifest
    {
        public enum ActivityExecutionPhase
        {
            WasNotExecuted = 0,
            ProcessingChildren = 1               
        }

        public class StackFrame
        {
            public Guid NodeId { get; set; }
            public string NodeName { get; set; }
            public ActivityExecutionPhase CurrentActivityExecutionPhase { get; set; }
            public Guid? CurrentChildId { get; set; }
        }

        public class HistoryElement
        {
            public string Description { get; set; }
        }

        public class LoopStatus
        {
            public string Id { get; set; }
            public int Index { get; set; }
            public bool BreakSignalReceived { get; set; }
            public int Level { get; set; }
            public string CrateManifest { get; set; }
            public string Label { get; set; }
        }

        public class BranchStatus
        {
            public string Id { get; set; }
            public int Count { get; set; }
            public DateTime LastBranchTime { get; set; }
        }

        public Stack<StackFrame> CallStack { get; set; } = new Stack<StackFrame>();

        public List<LoopStatus> Loops { get; set; }
        public List<BranchStatus> Branches { get; set; }
        public List<HistoryElement> History { get; set; }
        public ActivityErrorCode? CurrentActivityErrorCode { get; set; }
        public string CurrentActivityErrorMessage { get; set; }
        public string CurrentClientActivityName { get; set; }

        public ActivityResponseDTO CurrentActivityResponse { get; set; }

        public OperationalStateCM()
            : base(MT.OperationalStatus)
        {
            Loops = new List<LoopStatus>();
            Branches = new List<BranchStatus>();
            History = new List<HistoryElement>();
        }

    }
}
