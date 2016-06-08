using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IReport
    {
        HistoryResultDTO<IncidentDTO> GetIncidents(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO);
        HistoryResultDTO<FactDTO> GetFacts(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO);
    }
}
