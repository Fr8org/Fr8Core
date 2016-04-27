using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IReport
    {
        HistoryResultDTO<IncidentDTO> GetIncidents(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO);
        HistoryResultDTO<FactDTO> GetFacts(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO);
    }
}
