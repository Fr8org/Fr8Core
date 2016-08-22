using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public interface IActivityCategoryRepository : IGenericRepository<ActivityCategoryDO>
    {
    }

    public class ActivityCategoryRepository : GenericRepository<ActivityCategoryDO>, IActivityCategoryRepository
    {
        public ActivityCategoryRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
