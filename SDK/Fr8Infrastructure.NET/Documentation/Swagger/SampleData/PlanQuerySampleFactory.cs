using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PlanQuerySampleFactory : ISwaggerSampleFactory<PlanQueryDTO>
    {
        public PlanQueryDTO GetSampleData()
        {
            return new PlanQueryDTO
            {
                Id = Guid.Parse("D4AFC54F-5D09-4160-B498-A5FC1E91021F"),
                Category = "Solution",
                Filter = "part_of_name",
                IsDescending = false,
                OrderBy = "-name",
                Page = 1,
                PlanPerPage = 50,
                Status = "Inactive"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}