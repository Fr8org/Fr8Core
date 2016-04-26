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
        IList<FactDO> GetAllFacts(IUnitOfWork uow);
        List<IncidentDO> GetIncidents(IUnitOfWork uow, int page, int pageSize, bool isCurrentUser);
        HistoryResultDTO GetIncidents(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO);
    }
}
