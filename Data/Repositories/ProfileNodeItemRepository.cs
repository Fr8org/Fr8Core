using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class ProfileItemRepository : GenericRepository<ProfileItemDO>, IProfileItemRepository
    {
        internal ProfileItemRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }


    public interface IProfileItemRepository : IGenericRepository<ProfileItemDO>
    {

    }
}
