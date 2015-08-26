
using System.IO;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class FileRepository : GenericRepository<FileDO>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow) : base(uow)
        {
            
        }

        /// <see cref="IFileRepository.SaveRemoteFile"/>
        public string SaveRemoteFile(FileStream remoteFile)
        {
            return string.Empty;
        }

        /// <see cref="IFileRepository.GetRemoteFile"/>
        public FileStream GetRemoteFile(string blobUrl)
        {
            return null;
        }

        /// <see cref="IFileRepository.DeleteRemoteFile"/>
        public bool DeleteRemoteFile(string blobUrl)
        {
            return false;
        }
    }

    /// <summary>
    /// Repository for FileDO
    /// </summary>
    public interface IFileRepository : IGenericRepository<FileDO>
    {
        /// <summary>
        /// Saves a new BLOB in Azure Storage
        /// </summary>
        /// <param name="remoteFile">File Stream to be stored in remote Azure Storage</param>
        /// <returns>Azure Storage URL of the saved file</returns>
        string SaveRemoteFile(FileStream remoteFile);

        /// <summary>
        /// Retrieves a file stream from the Azure Storage
        /// </summary>
        /// <param name="blobUrl">URL of the existing BLOB</param>
        /// <returns>File Stream of the file</returns>
        FileStream GetRemoteFile(string blobUrl);

        /// <summary>
        /// Deletes BLOB in Azure Storage for the given URL
        /// </summary>
        /// <param name="blobUrl">URL of the BLOB to be deleted</param>
        /// <returns>True if the BLOB is deleted, False otherwise</returns>
        bool DeleteRemoteFile(string blobUrl);
    }
}
