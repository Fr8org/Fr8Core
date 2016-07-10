using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger.SampleData
{
    public class ActivitySampleFactory : ISwaggerSampleFactory<ActivityDTO>
    {
        private readonly ISwaggerSampleFactory<ActivityTemplateDTO> _activityTemplateFactory;

        private readonly Lazy<ActivityDTO> _factory;

        public ActivitySampleFactory(ISwaggerSampleFactory<ActivityTemplateDTO> activityTemplateFactory)
        {
            _activityTemplateFactory = activityTemplateFactory;
            _factory = new Lazy<ActivityDTO>(() => new ActivityDTO
            {
                Name = "Send_Message_v1",
                Label = "Send Message",
                Id = Guid.Parse("06D08982-6337-4E6F-AA59-0210D17F6861"),
                ParentPlanNodeId = Guid.Parse("4C21492F-80B6-4495-9602-E38697B916B2"),
                ActivityTemplate = _activityTemplateFactory.GetSampleData(),
                AuthToken = null,
                AuthTokenId = Guid.Parse("E75ADBF4-0A31-4C11-86F5-2AD41D54AC52"),
                ChildrenActivities = new ActivityDTO[0],
                CrateStorage = new CrateStorageDTO(),
                Fr8AccountId = "2BCD3978-38CE-43EB-8721-BC81B644B6ED",
                Ordering = 1,
                RootPlanNodeId = Guid.Parse("6D3FFCE0-2FE1-48D9-B046-73D078186E2E"),
            });
        }

        public ActivityDTO GetSampleData()
        {
            return _factory.Value;
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}