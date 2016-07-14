using System;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace HubWeb.Documentation.Swagger
{
    public class PlanFullSampleFactory : ISwaggerSampleFactory<PlanFullDTO>
    {
        private readonly ISwaggerSampleFactory<FullSubplanDto> _fullSubplanSampleFactory;
        public PlanFullSampleFactory(ISwaggerSampleFactory<FullSubplanDto> fullSubplanSampleFactory)
        {
            _fullSubplanSampleFactory = fullSubplanSampleFactory;
        }

        public PlanFullDTO GetSampleData()
        {
            return new PlanFullDTO
            {
                Id = Guid.Parse("B8E50730-6420-4DE0-B8B7-956D1A7AF1EF"),
                Name = "Plan Name",
                Description = "Plan Description",
                LastUpdated = DateTimeOffset.Now,
                Category = "Solutions",
                PlanState = PlanState.Inactive,
                StartingSubPlanId = Guid.Parse("39080509-1A69-43E6-910F-38C84B84324C"),
                Tag = "some tags",
                Visibility = PlanVisibility.Standard,
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