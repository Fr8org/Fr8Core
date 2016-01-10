using Data.Entities;
using Data.Interfaces;
using Data.Utility.JoinClasses;

namespace Data.Repositories
{
    public class FileTagsRepository : GenericRepository<FileTags>, IFileTagsRepository
    {
        internal FileTagsRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public interface IFileTagsRepository : IGenericRepository<FileTags>
    {

    }
}
