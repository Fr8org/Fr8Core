using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class TerminalSubscriptionRepository : GenericRepository<TerminalSubscriptionDO>, ITerminalSubscriptionRepository
    {
        public TerminalSubscriptionRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }

    public interface ITerminalSubscriptionRepository : IGenericRepository<TerminalSubscriptionDO>
    {
    }
}
