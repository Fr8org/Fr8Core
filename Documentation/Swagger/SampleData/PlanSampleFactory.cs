using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class PlanSampleFactory : ISwaggerSampleFactory<PlanDTO>
    {
        private readonly ISwaggerSampleFactory<PlanDTO> _planFullSampleFactory;

        public PlanSampleFactory(ISwaggerSampleFactory<PlanDTO> planFullSampleFactory)
        {
            _planFullSampleFactory = planFullSampleFactory;
        }

        public PlanDTO GetSampleData()
        {
            return _planFullSampleFactory.GetSampleData();
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}