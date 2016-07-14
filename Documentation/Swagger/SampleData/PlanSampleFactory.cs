using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class PlanSampleFactory : ISwaggerSampleFactory<PlanDTO>
    {
        private readonly ISwaggerSampleFactory<PlanFullDTO> _planFullSampleFactory;

        public PlanSampleFactory(ISwaggerSampleFactory<PlanFullDTO> planFullSampleFactory)
        {
            _planFullSampleFactory = planFullSampleFactory;
        }

        public PlanDTO GetSampleData()
        {
            return new PlanDTO
            {
                Plan = _planFullSampleFactory.GetSampleData()
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}