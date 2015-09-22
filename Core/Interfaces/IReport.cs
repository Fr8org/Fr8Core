using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IReport
    {
        List<FactDO> GetAllFacts(IUnitOfWork uow);
        List<IncidentDO> GetAllIncidents(IUnitOfWork uow);
    }
}
