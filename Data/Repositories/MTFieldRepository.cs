using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTFieldRepository : GenericRepository<MT_FieldDO>, IMTFieldRepository
    {
        internal MTFieldRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}
