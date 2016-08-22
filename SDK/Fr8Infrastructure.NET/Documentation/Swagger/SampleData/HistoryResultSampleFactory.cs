using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class HistoryResultIcidentSampleFactory : ISwaggerSampleFactory<PagedResultDTO<IncidentDTO>>
    {
        private readonly ISwaggerSampleFactory<IncidentDTO> _incidentSampleFactory;

        public HistoryResultIcidentSampleFactory(ISwaggerSampleFactory<IncidentDTO> incidentSampleFactory)
        {
            _incidentSampleFactory = incidentSampleFactory;
        }

        public PagedResultDTO<IncidentDTO> GetSampleData()
        {
            return new PagedResultDTO<IncidentDTO>
            {
                CurrentPage = 1,
                Items = new List<IncidentDTO> { _incidentSampleFactory.GetSampleData() },
                TotalItemCount = 150
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }

    public class HistoryResultFactSampleFactory : ISwaggerSampleFactory<PagedResultDTO<FactDTO>>
    {
        private readonly ISwaggerSampleFactory<FactDTO> _factSampleFactory;

        public HistoryResultFactSampleFactory(ISwaggerSampleFactory<FactDTO> factSampleFactory)
        {
            _factSampleFactory = factSampleFactory;
        }

        public PagedResultDTO<FactDTO> GetSampleData()
        {
            return new PagedResultDTO<FactDTO>
            {
                CurrentPage = 1,
                Items = new List<FactDTO> { _factSampleFactory.GetSampleData() },
                TotalItemCount = 150
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}