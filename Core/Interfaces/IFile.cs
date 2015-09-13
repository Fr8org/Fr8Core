
using System.IO;
using Data.Entities;

namespace Core.Interfaces
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
        void Store(FileDO curFileDO, Stream curFile, string curFileName);

        /// <summary>
        /// Retrieves file from repository
        /// </summary>
        byte[] Retrieve(FileDO curFile);

        /// <summary>
        /// Deletes file from repository
        /// </summary>
        bool Delete(FileDO curFile);
    }
}
