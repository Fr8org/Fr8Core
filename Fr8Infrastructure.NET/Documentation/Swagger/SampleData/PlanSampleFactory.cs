using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PlanSampleFactory : ISwaggerSampleFactory<PlanDTO>
    {
        private readonly ISwaggerSampleFactory<FullSubplanDto> _fullSubplanSampleFactory;
        public PlanSampleFactory(ISwaggerSampleFactory<FullSubplanDto> fullSubplanSampleFactory)
        {
            _fullSubplanSampleFactory = fullSubplanSampleFactory;
        }

        public PlanDTO GetSampleData()
        {
            return new PlanDTO
            {
                Id = Guid.Parse("B8E50730-6420-4DE0-B8B7-956D1A7AF1EF"),
                Name = "Plan Name",
                Description = "Plan Description",
                LastUpdated = DateTimeOffset.Now,
                Category = "Solutions",
                PlanState = "Inactive",
                StartingSubPlanId = Guid.Parse("39080509-1A69-43E6-910F-38C84B84324C"),
                Tag = "some tags",
                Visibility = new PlanVisibilityDTO() { Hidden = false },
                Fr8UserId = "A41D5268-7DE0-4802-9F64-08E2D7D49375",
                SubPlans = new []{ _fullSubplanSampleFactory.GetSampleData() }
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}