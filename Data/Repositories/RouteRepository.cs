using System.Linq;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;

namespace Data.Repositories
{
    public class RouteRepository : GenericRepository<RouteDO>, IRouteRepository
    {
        internal RouteRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IRouteRepository : IGenericRepository<RouteDO>
    {

    }
}