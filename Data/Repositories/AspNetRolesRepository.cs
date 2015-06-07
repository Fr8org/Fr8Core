using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class AspNetRolesRepository : GenericRepository<AspNetRolesDO>, IAspNetRolesRepository
    {

        internal AspNetRolesRepository(IUnitOfWork uow)
            : base(uow)
        {
            
        }

    }

    public interface IAspNetRolesRepository : IGenericRepository<AspNetRolesDO>
    {
        
    }
}
