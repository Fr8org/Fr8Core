using Data.Entities;
using Data.Interfaces;
using Data.Utility;

namespace Data.Repositories
{
    public class TagRepository : GenericRepository<TagDO>, ITagRepository
    {
        internal TagRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface ITagRepository : IGenericRepository<TagDO>
    {

    }
}
