using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class PlanResultSampleFactory : ISwaggerSampleFactory<PlanResultDTO>
    {
        private readonly ISwaggerSampleFactory<PlanEmptyDTO> _planEmptySampleFactory;
        public PlanResultSampleFactory(ISwaggerSampleFactory<PlanEmptyDTO> planEmptySampleFactory)
        {
            _planEmptySampleFactory = planEmptySampleFactory;
        }

        public PlanResultDTO GetSampleData()
        {
            return new PlanResultDTO
            {
                CurrentPage = 1,
                Plans = new List<PlanEmptyDTO> {_planEmptySampleFactory.GetSampleData()},
                TotalPlanCount = 200
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}