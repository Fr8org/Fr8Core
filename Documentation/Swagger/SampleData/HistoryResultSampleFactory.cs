using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class HistoryResultIcidentSampleFactory : ISwaggerSampleFactory<HistoryResultDTO<IncidentDTO>>
    {
        private readonly ISwaggerSampleFactory<IncidentDTO> _incidentSampleFactory;

        public HistoryResultIcidentSampleFactory(ISwaggerSampleFactory<IncidentDTO> incidentSampleFactory)
        {
            _incidentSampleFactory = incidentSampleFactory;
        }

        public HistoryResultDTO<IncidentDTO> GetSampleData()
        {
            return new HistoryResultDTO<IncidentDTO>
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

    public class HistoryResultFactSampleFactory : ISwaggerSampleFactory<HistoryResultDTO<FactDTO>>
    {
        private readonly ISwaggerSampleFactory<FactDTO> _factSampleFactory;

        public HistoryResultFactSampleFactory(ISwaggerSampleFactory<FactDTO> factSampleFactory)
        {
            _factSampleFactory = factSampleFactory;
        }

        public HistoryResultDTO<FactDTO> GetSampleData()
        {
            return new HistoryResultDTO<FactDTO>
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