using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class NegotiationAnswerEmailRepository : GenericRepository<NegotiationAnswerEmailDO>, INegotiationAnswerEmailRepository
    {
        public NegotiationAnswerEmailRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }
    public interface INegotiationAnswerEmailRepository : IGenericRepository<NegotiationAnswerEmailDO>
    {

    }
}
