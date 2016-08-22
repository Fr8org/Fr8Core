using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{


    public class RecipientRepository : GenericRepository<RecipientDO>, IRecipientRepository
    {

        internal RecipientRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IRecipientRepository : IGenericRepository<RecipientDO>
    {
    }
}
