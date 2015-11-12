using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using System.IO;
using System;

namespace Data.Repositories
{
    public class FileRepository : GenericRepository<FileDO>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow) : base(uow)
        {
        }

        public string SaveRemoteFile(MemoryStream ms, string v)
        {
            throw new NotImplementedException();
        }

        public void DeleteRemoteFile(string url)
        {
            throw new NotImplementedException();
        }
    }
}
 