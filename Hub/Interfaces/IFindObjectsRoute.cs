using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IFindObjectsRoute
    {
        PlanDO CreatePlan(IUnitOfWork uow, Fr8AccountDO account);
    }
}
