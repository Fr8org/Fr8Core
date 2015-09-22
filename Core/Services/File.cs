
using System.IO;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using System;

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
        public void Store(FileDO curFileDO, Stream curFile, string curFileName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string remoteFileUrl = uow.FileRepository.SaveRemoteFile(curFile, curFileName);

                curFileDO.CloudStorageUrl = remoteFileUrl;
                curFileDO.OriginalFileName = curFileName;
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
                    if(uow.Db.Entry<FileDO>(curFile).State == System.Data.Entity.EntityState.Detached)
                        uow.Db.Set<FileDO>().Attach(curFile);
                    uow.FileRepository.Remove(curFile);
                    uow.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public bool Delete(int fileId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var file = uow.FileRepository.GetByKey(fileId);
                if (null != file)
                {
                    return Delete(file);;
                }
                return true;
            }
        }

        public IList<FileDO> AllFilesList()
        {
            return FilesList(null);
        }

        public IList<FileDO> FilesList(string dockyardAccountId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<FileDO> files;
                if (String.IsNullOrEmpty(dockyardAccountId))
                    files = uow.FileRepository.GetAll().ToList();
                else
                    files = uow.FileRepository.FindList(f => f.DockyardAccountID == dockyardAccountId).ToList();
                return files;
            }
        }
    }
}
