using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ActivitySampleFactory : ISwaggerSampleFactory<ActivityDTO>
    {
        private readonly ISwaggerSampleFactory<ActivityTemplateSummaryDTO> _activityTemplateFactory;

        public ActivitySampleFactory(ISwaggerSampleFactory<ActivityTemplateSummaryDTO> activityTemplateFactory)
        {
            _activityTemplateFactory = activityTemplateFactory;
        }

        public ActivityDTO GetSampleData()
        {
            return new ActivityDTO
            {
                Label = "Send Message",
                Id = Guid.Parse("06D08982-6337-4E6F-AA59-0210D17F6861"),
                ParentPlanNodeId = Guid.Parse("4C21492F-80B6-4495-9602-E38697B916B2"),
                ActivityTemplate = _activityTemplateFactory.GetSampleData(),
                AuthToken = null,
                AuthTokenId = Guid.Parse("E75ADBF4-0A31-4C11-86F5-2AD41D54AC52"),
                ChildrenActivities = new ActivityDTO[0],
                CrateStorage = new CrateStorageDTO(),
                Ordering = 1,
                RootPlanNodeId = Guid.Parse("6D3FFCE0-2FE1-48D9-B046-73D078186E2E"),
                Documentation = "MainPage"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}