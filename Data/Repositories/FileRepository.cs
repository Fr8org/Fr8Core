using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class FileRepository : GenericRepository<FileDO>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow) : base(uow)
        {
        }
    }
}
