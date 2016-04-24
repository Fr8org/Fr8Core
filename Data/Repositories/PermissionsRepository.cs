using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class PermissionRepository : GenericRepository<PermissionDO>, IPermissionRepository
    {
        public PermissionRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface IPermissionRepository : IGenericRepository<PermissionDO>
    {
    }
}
