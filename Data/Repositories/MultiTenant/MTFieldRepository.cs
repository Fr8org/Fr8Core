using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTFieldRepository : GenericRepository<MT_Field>, IMTFieldRepository
    {
        internal MTFieldRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}
