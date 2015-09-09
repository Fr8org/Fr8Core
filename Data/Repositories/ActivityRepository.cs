using Data.Entities;
using Data.Interfaces;
using Data.States;

namespace Data.Repositories
{
    public class ActivityRepository : GenericRepository<ActivityDO>, IActivityRepository
    {
		 public ActivityRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IActivityRepository : IGenericRepository<ActivityDO>
    {

    }
}