using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IReport
    {
        PagedResultDTO<IncidentDTO> GetIncidents(IUnitOfWork uow, PagedQueryDTO pagedQueryDto);
        PagedResultDTO<FactDTO> GetFacts(IUnitOfWork uow, PagedQueryDTO pagedQueryDto);
    }
}
