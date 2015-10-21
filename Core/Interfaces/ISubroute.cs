using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    /// <summary>
    /// Subroute service.
    /// </summary>
    public interface ISubroute
    {
        void Create(IUnitOfWork uow, SubrouteDO subroute);
        void Update(IUnitOfWork uow, SubrouteDO subroute);
        void Delete(IUnitOfWork uow, int id);
        void AddAction(IUnitOfWork uow, ActionDO resultActionDo);
        Task<string> DeleteAction(int id, bool confirmed);
    }
}
