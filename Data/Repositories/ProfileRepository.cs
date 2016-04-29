using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProfileRepository : GenericRepository<ProfileDO>, IProfileRepository
    {
        public ProfileRepository(IUnitOfWork uow)
            : base(uow)
        {
        }
    }

    public interface IProfileRepository : IGenericRepository<ProfileDO>
    {
    }
}
