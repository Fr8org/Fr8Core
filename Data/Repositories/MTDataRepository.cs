using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTDataRepository : GenericRepository<MT_DataDO>, IMTDataRepository
    {
        internal MTDataRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}
