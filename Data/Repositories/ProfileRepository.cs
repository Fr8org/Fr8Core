using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProfileRepository : GenericRepository<ProfileDO>, IProfileRepository
    {
        internal ProfileRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IProfileRepository : IGenericRepository<ProfileDO>
    {

    }
}
