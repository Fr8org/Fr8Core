using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class QuerySampleFactory : ISwaggerSampleFactory<QueryDTO>
    {
        private readonly ISwaggerSampleFactory<FilterConditionDTO> _filterConditionSampleFactory;
        public QuerySampleFactory(ISwaggerSampleFactory<FilterConditionDTO> filterConditionSampleFactory)
        {
            _filterConditionSampleFactory = filterConditionSampleFactory;
        }

        public QueryDTO GetSampleData()
        {
            return new QueryDTO
            {
                Name = "Some fiter",
                Criteria = new List<FilterConditionDTO> { _filterConditionSampleFactory.GetSampleData() }
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}