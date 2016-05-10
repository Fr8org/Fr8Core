using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace terminalBox.Infrastructure
{
    public interface IBoxService
    {
        /// <summary>
        /// Represents a dictionary of folder names accessed by ids.
        /// </summary>
        /// <returns></returns>
        Task<ReadOnlyDictionary<string, string>> GetFolderNames();

        /// <summary>
        /// Saves file into box storage.
        /// </summary>
        /// <param name="fileName">Name of the file to save</param>
        /// <param name="content">File content</param>
        Task<string> SaveFile(string fileName, Stream content);

        /// <summary>
        /// Returns file download link.
        /// </summary>
        /// <param name="id">Id of the file to download</param>
        /// <returns></returns>
        Task<string> GetFileLink(string id);

        /// <summary>
        /// Returns current user login.
        /// </summary>
        /// <returns></returns>
        Task<string> GetCurrentUserLogin();
    }
}