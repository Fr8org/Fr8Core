using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PlanEmptySampleFactory : ISwaggerSampleFactory<PlanNoChildrenDTO>
    {
        public PlanNoChildrenDTO GetSampleData()
        {
            return new PlanNoChildrenDTO
            {
                Id = Guid.Parse("B8E50730-6420-4DE0-B8B7-956D1A7AF1EF"),
                Name = "Plan Name",
                Description = "Plan Description",
                LastUpdated = DateTimeOffset.Now,
                Category = "Solutions",
                PlanState = "Inactive",
                StartingSubPlanId = Guid.Parse("39080509-1A69-43E6-910F-38C84B84324C"),
                Tag = "some tags",
                Visibility = new PlanVisibilityDTO() { Hidden = false }
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}