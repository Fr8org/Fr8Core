using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class SubplanSampleFactory : ISwaggerSampleFactory<SubplanDTO>
    {
        public SubplanDTO GetSampleData()
        {
            return new SubplanDTO
            {
                Name = "Starting Subplan",
                PlanId = Guid.Parse("51862543-BD9C-429A-B0E1-79F3CC333E89"),
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