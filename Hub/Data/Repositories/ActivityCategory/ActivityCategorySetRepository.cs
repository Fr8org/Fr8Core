using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public interface IActivityCategorySetRepository : IGenericRepository<ActivityCategorySetDO>
    {
    }

    public class ActivityCategorySetRepository : GenericRepository<ActivityCategorySetDO>, IActivityCategorySetRepository
    {
        public ActivityCategorySetRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
