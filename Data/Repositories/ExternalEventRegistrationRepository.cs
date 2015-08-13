using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ExternalEventRegistrationRepository : GenericRepository<ExternalEventSubscriptionDO>, IExternalEventRegistrationRepository
    {
        public ExternalEventRegistrationRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public new void Add(ExternalEventSubscriptionDO entity)
        {
            base.Add(entity);
        }
    }

    public interface IExternalEventRegistrationRepository : IGenericRepository<ExternalEventSubscriptionDO>
    {

    }
}