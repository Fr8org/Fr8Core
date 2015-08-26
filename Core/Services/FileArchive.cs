
using System.IO;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// File Archive service
    /// </summary>
    public class FileArchive : IFileArchive
    {
        public FileArchive()
        {
            
        }

        /// <see cref="IFileArchive.WriteFile"/>
        public void WriteFile(FileDO fileDo, FileStream file, string fileName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string remoteFileUrl =
                    uow.FileRepository.SaveRemoteFile(file, fileName);

                fileDo.CloudStorageUrl = remoteFileUrl;

                uow.FileRepository.Add(fileDo);
                uow.SaveChanges();
            }
        }

        /// <see cref="IFileArchive.ReadFile"/>
        public byte[] ReadFile(FileDO curFile)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.FileRepository.GetRemoteFile(curFile.CloudStorageUrl);
            }
        }

        /// <see cref="IFileArchive.DeleteFile"/>
        public bool DeleteFile(FileDO curFile)
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
