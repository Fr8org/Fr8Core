using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IFindObjectsRoute
    {
        RouteDO CreateRoute(IUnitOfWork uow, Fr8AccountDO account);
    }
}
