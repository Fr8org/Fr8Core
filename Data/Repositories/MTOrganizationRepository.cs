using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class MTOrganizationRepository : GenericRepository<MT_OrganizationDO>, IMTOrganizationRepository
    {
        internal MTOrganizationRepository(IUnitOfWork uow) : base(uow)
        {
            
        }
    }
}
