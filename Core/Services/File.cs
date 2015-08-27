
using System.IO;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class File : IFile
    {
        /// <summary>
        /// Stores the file into file repository
        /// </summary>
        /// <remarks>WARNING: THIS METHOD IS NOT TRANSACTIONAL. It is possible to successfuly save to the remote store and then have the FileDO update fail.</remarks>
        public void Store(FileDO curFileDO, FileStream curFile, string curFileName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string remoteFileUrl = uow.FileRepository.SaveRemoteFile(curFile, curFileName);

                curFileDO.CloudStorageUrl = remoteFileUrl;

                uow.FileRepository.Add(curFileDO);
                uow.SaveChanges();
            }
        }

        public byte[] Retrieve(FileDO curFile)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.FileRepository.GetRemoteFile(curFile.CloudStorageUrl);
            }
        }

        public bool Delete(FileDO curFile)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                bool isRemoteFileDeleted = uow.FileRepository.DeleteRemoteFile(curFile.CloudStorageUrl);

                if (isRemoteFileDeleted)
                {
                    uow.FileRepository.Remove(curFile);
                    uow.SaveChanges();
                    return true;
                }

                return false;
            }
        }
    }
}
