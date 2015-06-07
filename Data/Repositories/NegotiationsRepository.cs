using Data.Interfaces;
using Data.Entities;

namespace Data.Repositories
{
    public class NegotiationsRepository : GenericRepository<NegotiationDO>, INegotiationsRepository
    {
        internal NegotiationsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface INegotiationsRepository : IGenericRepository<NegotiationDO>
    {

    }
}
