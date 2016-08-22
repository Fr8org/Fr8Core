using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Hub.Interfaces;

namespace Hub.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class File : IFile
    {
        private readonly CloudFileManager _cloudFileManager
            = ObjectFactory.GetInstance<CloudFileManager>();

        /// <summary>
        /// Stores the file into file repository
        /// </summary>
        /// <remarks>WARNING: THIS METHOD IS NOT TRANSACTIONAL. It is possible to successfuly save to the remote store and then have the FileDO update fail.</remarks>
        public void Store(IUnitOfWork uow, FileDO curFileDO, Stream curFile, string curFileName)
        {
            var existingFile = FilesList(curFileDO.DockyardAccountID).FirstOrDefault(x => x.OriginalFileName == curFileName);
            if (existingFile != null)
            {
                Delete(existingFile);
            }
            var remoteFileUrl = _cloudFileManager.SaveRemoteFile(curFile, curFileName);

            curFileDO.CloudStorageUrl = remoteFileUrl;
            curFileDO.OriginalFileName = curFileName;
            uow.FileRepository.Add(curFileDO);
            uow.SaveChanges();
        }

        public byte[] Retrieve(FileDO curFile)
        {
            return _cloudFileManager.GetRemoteFile(curFile.CloudStorageUrl);
        }

        public bool Delete(FileDO curFile)
        {
            return Delete(curFile.Id);
        }

        public bool Delete(int fileId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var file = uow.FileRepository.GetByKey(fileId);

                if (null != file)
                {
                    bool isRemoteFileDeleted = _cloudFileManager.DeleteRemoteFile(file.CloudStorageUrl);

                    if (isRemoteFileDeleted)
                    {
                        /*if (uow.Db.Entry(curFile).State == System.Data.Entity.EntityState.Detached)
                            //if(uow.Db.Entry<FileDO>(curFile).State == System.Data.Entity.EntityState.Detached)
                            uow.Db.Set<FileDO>().Attach(curFile);*/
                        uow.FileRepository.Remove(file);
                        uow.SaveChanges();
                        return true;
                    }
                    return false;
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

        public FileDO GetFileByAdmin (int fileId)
        {
            return GetFile(fileId, null);
        }

        public FileDO GetFile(string filePath, string dockyardAccountId)
        {
            FileDO file = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                file = uow.FileRepository.GetQuery().FirstOrDefault(f => f.CloudStorageUrl == filePath);
                if (file != null)
                {
                    if (dockyardAccountId != null && file.DockyardAccountID != dockyardAccountId)
                    {
                        return null;
                    }
                }

                return file;
            }
        }

        public FileDO GetFile(int fileId, string dockyardAccountId)
        {
            FileDO file = null;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                file = uow.FileRepository.GetByKey(fileId);
                if (file != null)
                {
                    if (dockyardAccountId != null && file.DockyardAccountID != dockyardAccountId)
                    {
                        return null;
                    }
                }

                return file;
            }
        }
    }
}