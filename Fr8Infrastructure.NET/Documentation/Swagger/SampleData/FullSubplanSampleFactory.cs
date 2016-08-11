using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class FullSubplanSampleFactory : ISwaggerSampleFactory<FullSubplanDto>
    {
        private readonly ISwaggerSampleFactory<ActivityDTO> _activitySampleFactory;
        public FullSubplanSampleFactory(ISwaggerSampleFactory<ActivityDTO> activitySampleFactory)
        {
            _activitySampleFactory = activitySampleFactory;
        }

        public FullSubplanDto GetSampleData()
        {
            return new FullSubplanDto
            {
                Name = "Starting Subplan",
                PlanId = Guid.Parse("51862543-BD9C-429A-B0E1-79F3CC333E89"),
                Activities = new List<ActivityDTO> {  _activitySampleFactory.GetSampleData() },
                ParentId = Guid.Parse("51862543-BD9C-429A-B0E1-79F3CC333E89"),
                SubPlanId = Guid.Parse("8A38FEF4-800F-4BB1-95EC-2B2805A4FB29"),
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}