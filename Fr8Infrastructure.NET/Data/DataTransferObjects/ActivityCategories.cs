using System;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public static class ActivityCategories
    {
        private readonly static ActivityCategoryDTO _monitor = new ActivityCategoryDTO()
        {
            Id = MonitorId,
            Name = MonitorName,
            IconPath = "/Content/icons/monitor-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _receive = new ActivityCategoryDTO()
        {
            Id = ReceiveId,
            Name = ReceiveName,
            IconPath = "/Content/icons/get-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _process = new ActivityCategoryDTO()
        {
            Id = ProcessId,
            Name = ProcessName,
            IconPath = "/Content/icons/process-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _forward = new ActivityCategoryDTO()
        {
            Id = ForwardId,
            Name = ForwardName,
            IconPath = "/Content/icons/forward-icon-64x64.png"
        };

        private readonly static ActivityCategoryDTO _solution = new ActivityCategoryDTO()
        {
            Id = SolutionId,
            Name = SolutionName,
            IconPath = "/Content/icons/solution-icon-64x64.png"
        };

        private readonly static List<Guid> _categoryIds = new List<Guid>()
        {
            MonitorId,
            ReceiveId,
            ProcessId,
            ForwardId,
            SolutionId
        };

        private readonly static List<ActivityCategoryDTO> _categories = new List<ActivityCategoryDTO>()
        {
            Monitor,
            Receive,
            Process,
            Forward,
            Solution
        };


        public static Guid MonitorId
        {
            get { return Guid.Parse("417DD061-27A1-4DEC-AECD-4F468013FD24"); }
        }

        public static string MonitorName
        {
            get { return "Triggers"; }
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
            get { return "Get Data"; }
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
            get { return "Ship Data"; }
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

        public static List<ActivityCategoryDTO> ActivityCategoryList
        {
            get { return _categories; }
        }
    }
}
