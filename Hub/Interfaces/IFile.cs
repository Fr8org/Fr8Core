using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    /// <summary>
    /// Interface for File service
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Stores the file into file repository
        /// </summary>
        /// <remarks>WARNING: THIS METHOD IS NOT TRANSACTIONAL. It is possible to successfuly save to the remote store and then have the FileDO update fail.</remarks>
        void Store(IUnitOfWork uow, FileDO curFileDO, Stream curFile, string curFileName);

        /// <summary>
        /// Retrieves file from repository
        /// </summary>
        byte[] Retrieve(FileDO curFile);

        /// <summary>
        /// Deletes file from repository
        /// </summary>
        bool Delete(FileDO curFile);

        /// <summary>
        /// Deletes file from repository by Id
        /// </summary>
        bool Delete(int fileId);

        /// <summary>
        /// Retreive list of files for specified DockyardAccount
        /// </summary>
        IList<FileDO> FilesList(string dockyardAccountId);

        /// <summary>
        /// Retreive list of all files 
        /// </summary>
        /// <returns></returns>
        IList<FileDO> AllFilesList();

        /// <summary>
        /// Retreive file details by admin
        /// </summary>
        /// <returns></returns>
        FileDO GetFileByAdmin(int fileId);

        /// <summary>
        /// Retreive file details
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="dockyardAccountId"></param>
        /// <returns></returns>
        FileDO GetFile(int fileId, string dockyardAccountId);
        FileDO GetFile(string filePath, string dockyardAccountId);
    }
}
