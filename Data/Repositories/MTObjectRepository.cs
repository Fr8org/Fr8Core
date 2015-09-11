using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTObjectRepository : GenericRepository<MT_ObjectDO>, IMTObjectRepository
    {
        internal MTObjectRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}
