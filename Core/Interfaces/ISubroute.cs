using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    /// <summary>
    /// Subroute service.
    /// </summary>
    public interface ISubroute
    {
        void Create(IUnitOfWork uow, SubrouteDO subroute);
        SubrouteDO Create(IUnitOfWork uow);
        void Update(IUnitOfWork uow, SubrouteDO subroute);
        void Delete(IUnitOfWork uow, int id);
        void AddAction(IUnitOfWork uow, ActionDO resultActionDo);
        void DeleteAction(int id);
    }
}
