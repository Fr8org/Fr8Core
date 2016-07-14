using System;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace HubWeb.Documentation.Swagger
{
    public class PlanEmptySampleFactory : ISwaggerSampleFactory<PlanEmptyDTO>
    {
        public PlanEmptyDTO GetSampleData()
        {
            return new PlanEmptyDTO
            {
                Id = Guid.Parse("B8E50730-6420-4DE0-B8B7-956D1A7AF1EF"),
                Name = "Plan Name",
                Description = "Plan Description",
                LastUpdated = DateTimeOffset.Now,
                Category = "Solutions",
                PlanState = PlanState.Inactive,
                StartingSubPlanId = Guid.Parse("39080509-1A69-43E6-910F-38C84B84324C"),
                Tag = "some tags",
                Visibility = PlanVisibility.Standard
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}