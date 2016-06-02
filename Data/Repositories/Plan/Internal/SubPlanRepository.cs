using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class SubPlanRepository : GenericRepository<SubplanDO>, ISubPlanRepository
    {
        internal SubPlanRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface ISubPlanRepository : IGenericRepository<SubplanDO>
    {
    }
}