using System.Collections.Generic;
using System.IO;
using Data.Entities;

namespace Data.Interfaces
{
    /// <summary>
    /// Repository for FileDO
    /// </summary>
    public interface IFileRepository : IGenericRepository<FileDO>
    {
        /// <summary>
        /// Saves a new BLOB in Azure Storage
        /// </summary>
        /// <param name="curRemoteFile">File Stream to be stored in remote Azure Storage</param>
        /// <param name="curFileName">Name of the BLOB</param>
        /// <returns>Azure Storage URL of the saved file</returns>
        string SaveRemoteFile(Stream curRemoteFile, string curFileName);

        /// <summary>
        /// Retrieves a file stream from the Azure Storage
        /// </summary>
        /// <param name="curBlobUrl">URL of the existing BLOB</param>
        /// <returns>Bytes of the Blob File</returns>
        byte[] GetRemoteFile(string curBlobUrl);

        /// <summary>
        /// Deletes BLOB in Azure Storage for the given URL
        /// </summary>
        /// <param name="curBlobUrl">URL of the BLOB to be deleted</param>
        /// <returns>True if the BLOB is deleted, False otherwise</returns>
        bool DeleteRemoteFile(string curBlobUrl);
    }
}
