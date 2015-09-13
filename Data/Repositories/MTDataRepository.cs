using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTDataRepository : GenericRepository<MT_Data>, IMTDataRepository
    {
        internal MTDataRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}
