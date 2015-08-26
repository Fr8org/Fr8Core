
using System.IO;
using Core.Interfaces;
using Data.Entities;

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
        public void WriteFile(FileStream file)
        {
            
        }

        /// <see cref="IFileArchive.ReadFile"/>
        public FileStream ReadFile(FileDO curFile)
        {
            return null;
        }

        /// <see cref="IFileArchive.DeleteFile"/>
        public bool DeleteFile(FileDO curFile)
        {
            return false;
        }
    }
}
