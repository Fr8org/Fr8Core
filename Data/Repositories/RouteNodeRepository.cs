using Data.Entities;
using Data.Interfaces;
using Data.States;

namespace Data.Repositories
{
    public class RouteNodeRepository : GenericRepository<RouteNodeDO>, IRouteNodeRepository
    {
		 public RouteNodeRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IRouteNodeRepository : IGenericRepository<RouteNodeDO>
    {

    }
}