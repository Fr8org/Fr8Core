using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PlanResultSampleFactory : ISwaggerSampleFactory<PlanResultDTO>
    {
        private readonly ISwaggerSampleFactory<PlanNoChildrenDTO> _planEmptySampleFactory;
        public PlanResultSampleFactory(ISwaggerSampleFactory<PlanNoChildrenDTO> planEmptySampleFactory)
        {
            _planEmptySampleFactory = planEmptySampleFactory;
        }

        public PlanResultDTO GetSampleData()
        {
            return new PlanResultDTO
            {
                CurrentPage = 1,
                Plans = new List<PlanNoChildrenDTO> {_planEmptySampleFactory.GetSampleData()},
                TotalPlanCount = 200
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}