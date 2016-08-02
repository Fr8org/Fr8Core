using System;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public static class ActivityCategories
    {
        private readonly static ActivityCategoryDTO _monitor = new ActivityCategoryDTO()
        {
            Name = "Monitor",
            IconPath = "/Content/icons/monitor-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _receive = new ActivityCategoryDTO()
        {
            Name = "Get",
            IconPath = "/Content/icons/get-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _process = new ActivityCategoryDTO()
        {
            Name = "Process",
            IconPath = "/Content/icons/process-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _forward = new ActivityCategoryDTO()
        {
            Name = "Forward",
            IconPath = "/Content/icons/forward-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _solution = new ActivityCategoryDTO()
        {
            Name = "Solution"
        };

        private readonly static List<Guid> _categoryIds = new List<Guid>()
        {
            MonitorId,
            ReceiveId,
            ProcessId,
            ForwardId,
            SolutionId
        };
   
        public static Guid MonitorId
        {
            get { return Guid.Parse("417DD061-27A1-4DEC-AECD-4F468013FD24"); }
        }

        public static string MonitorName
        {
            get { return "Monitor"; }
        }

        public static ActivityCategoryDTO Monitor
        {
            get { return _monitor; }
        }

        public static Guid ReceiveId
        {
            get { return Guid.Parse("29EFB1D7-A9EA-41C5-AC60-AEF1F520E814"); }
        }

        public static string ReceiveName
        {
            get { return "Get"; }
        }

        public static ActivityCategoryDTO Receive
        {
            get { return _receive; }
        }

        public static Guid ProcessId
        {
            get { return Guid.Parse("69FB6D2C-2083-4696-9457-B7B152D358C2"); }
        }

        public static string ProcessName
        {
            get { return "Process"; }
        }

        public static ActivityCategoryDTO Process
        {
            get { return _process; }
        }

        public static Guid ForwardId
        {
            get { return Guid.Parse("AFD7E981-A21A-4B05-B0B1-3115A5448F22"); }
        }

        public static string ForwardName
        {
            get { return "Forward"; }
        }

        public static ActivityCategoryDTO Forward
        {
            get { return _forward; }
        }

        public static Guid SolutionId
        {
            get { return Guid.Parse("F9DF2AC2-2F10-4D21-B97A-987D46AD65B0"); }
        }

        public static string SolutionName
        {
            get { return "Solution"; }
        }

        public static ActivityCategoryDTO Solution
        {
            get { return _solution; }
        }

        public static List<Guid> ActivityCategoryIds
        {
            get { return _categoryIds; }
        }
    }
}
