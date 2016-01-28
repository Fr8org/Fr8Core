using System.Linq;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;

namespace Data.Repositories
{
    public class PlanRepository : GenericRepository<PlanDO>, IPlanRepository
    {
        internal PlanRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IPlanRepository : IGenericRepository<PlanDO>
    {

    }
}