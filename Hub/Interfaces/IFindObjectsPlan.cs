using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IFindObjectsPlan
    {
        PlanDO CreatePlan(IUnitOfWork uow, Fr8AccountDO account);
    }
}
