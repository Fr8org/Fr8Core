using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class PermissionSetRepository : GenericRepository<PermissionSetDO>, IPermissionSetRepository
    {
        public PermissionSetRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface IPermissionSetRepository : IGenericRepository<PermissionSetDO>
    {
    }
}
