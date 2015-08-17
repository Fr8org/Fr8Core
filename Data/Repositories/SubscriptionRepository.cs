using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class SubscriptionRepository : GenericRepository<SubscriptionDO>, ISubscriptionRepository
    {
        public SubscriptionRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface ISubscriptionRepository : IGenericRepository<SubscriptionDO>
    {
    }
}
