using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTFieldTypeRepository : GenericRepository<MT_FieldType>, IMTFieldTypeRepository
    {
        internal MTFieldTypeRepository(IUnitOfWork uow) : base(uow)
        {

        }
    }
}
