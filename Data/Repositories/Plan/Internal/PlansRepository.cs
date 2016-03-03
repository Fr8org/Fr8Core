using System.Linq;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;

namespace Data.Repositories
{
    public class PlansRepository : GenericRepository<PlanDO>
    {
        internal PlansRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    //    public interface IPlanRepository : IGenericRepository<PlanDO>
    //    {
    //
    //    }
}