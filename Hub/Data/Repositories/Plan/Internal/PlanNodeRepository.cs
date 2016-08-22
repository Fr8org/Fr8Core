using Data.Entities;
using Data.Interfaces;
using Data.States;

namespace Data.Repositories
{
    public class PlanNodeRepository : GenericRepository<PlanNodeDO>, IPlanNodeRepository
    {
        public PlanNodeRepository(IUnitOfWork uow)
           : base(uow)
        {

        }
    }

    public interface IPlanNodeRepository : IGenericRepository<PlanNodeDO>
    {

    }
}