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


        public static ActivityCategoryDTO Monitor
        {
            get { return _monitor; }
        }

        public static ActivityCategoryDTO Receive
        {
            get { return _receive; }
        }

        public static ActivityCategoryDTO Process
        {
            get { return _process; }
        }

        public static ActivityCategoryDTO Forward
        {
            get { return _forward; }
        }

        public static ActivityCategoryDTO Solution
        {
            get { return _solution; }
        }
    }
}
