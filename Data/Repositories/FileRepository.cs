
using System;
using System.Configuration;
using System.IO;
using Data.Entities;
using Data.Interfaces;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Data.Repositories
{
    public class FileRepository : GenericRepository<FileDO>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow) : base(uow)
        {
            
        }

        /// <see cref="IFileRepository.SaveRemoteFile"/>
        public string SaveRemoteFile(FileStream remoteFile, string fileName)
        {
            var blobContainer = GetDefaultBlobContainer();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
            blockBlob.UploadFromStream(remoteFile);

            return blockBlob.Uri.AbsoluteUri;
        }

        /// <see cref="IFileRepository.GetRemoteFile"/>
        public byte[] GetRemoteFile(string blobUrl)
        {
            var curBlob = new CloudBlockBlob(new Uri(blobUrl), GetDefaultBlobContainer().ServiceClient.Credentials);
            curBlob.FetchAttributes();

            byte[] content = new byte[curBlob.Properties.Length];
            curBlob.DownloadToByteArray(content, 0);
            
            return content;
        }

        /// <see cref="IFileRepository.DeleteRemoteFile"/>
        public bool DeleteRemoteFile(string blobUrl)
        {
            var curBlob = new CloudBlockBlob(new Uri(blobUrl), GetDefaultBlobContainer().ServiceClient.Credentials);
            return curBlob.DeleteIfExists();
        }

        private CloudBlobContainer GetDefaultBlobContainer()
        {
            const string storageConnectionString = "PrimaryFileStorageConnectionString";
            const string containerName = "container1";

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(storageConnectionString));

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container;
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
        /// <param name="fileName">Name of the BLOB</param>
        /// <returns>Azure Storage URL of the saved file</returns>
        string SaveRemoteFile(FileStream remoteFile, string fileName);

        /// <summary>
        /// Retrieves a file stream from the Azure Storage
        /// </summary>
        /// <param name="blobUrl">URL of the existing BLOB</param>
        /// <returns>Bytes of the Blob File</returns>
        byte[] GetRemoteFile(string blobUrl);

        /// <summary>
        /// Deletes BLOB in Azure Storage for the given URL
        /// </summary>
        /// <param name="blobUrl">URL of the BLOB to be deleted</param>
        /// <returns>True if the BLOB is deleted, False otherwise</returns>
        bool DeleteRemoteFile(string blobUrl);
    }
}
