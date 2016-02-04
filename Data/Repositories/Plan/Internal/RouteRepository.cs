using System.Linq;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;

namespace Data.Repositories
{
    public class RouteRepository : GenericRepository<PlanDO>
    {
        internal RouteRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

//    public interface IPlanRepository : IGenericRepository<PlanDO>
//    {
//
//    }
}