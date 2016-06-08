using System;
using System.IO;
using Fr8.Infrastructure.Utilities.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Data.Infrastructure
{
    public class CloudFileManager
    {
        /// <summary>
        /// Retrieves a file stream from the Azure Storage
        /// </summary>
        /// <param name="curBlobUrl">URL of the existing BLOB</param>
        /// <returns>Bytes of the Blob File</returns>
        public byte[] GetRemoteFile(string curBlobUrl)
        {
            var container = GetDefaultBlobContainer();

            if (curBlobUrl.StartsWith(container.Uri.ToString()))
            {
                var lastSlashOffset = (!container.Uri.ToString().EndsWith("/") ? 1 : 0);
                curBlobUrl = curBlobUrl.Substring(container.Uri.ToString().Length + lastSlashOffset);
            }

            var curBlob = container.GetBlobReference(curBlobUrl);
            curBlob.FetchAttributes();

            byte[] content = new byte[curBlob.Properties.Length];
            curBlob.DownloadToByteArray(content, 0, options: container.ServiceClient.DefaultRequestOptions);

            return content;
        }

        /// <summary>
        /// Saves a new BLOB in Azure Storage
        /// </summary>
        /// <param name="curRemoteFile">File Stream to be stored in remote Azure Storage</param>
        /// <param name="curFileName">Name of the BLOB</param>
        /// <returns>Azure Storage URL of the saved file</returns>
        public string SaveRemoteFile(Stream curRemoteFile, string curFileName)
        {
            var blobContainer = GetDefaultBlobContainer();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(curFileName);
            blockBlob.UploadFromStream(curRemoteFile);

            return Uri.UnescapeDataString(blockBlob.Uri.AbsoluteUri);
        }

        /// <summary>
        /// Deletes BLOB in Azure Storage for the given URL
        /// </summary>
        /// <param name="curBlobUrl">URL of the BLOB to be deleted</param>
        /// <returns>True if the BLOB is deleted, False otherwise</returns>
        public bool DeleteRemoteFile(string curBlobUrl)
        {
            var curBlob = new CloudBlockBlob(new Uri(curBlobUrl), GetDefaultBlobContainer().ServiceClient.Credentials);
            return curBlob.DeleteIfExists();
        }

        protected virtual CloudBlobContainer GetDefaultBlobContainer()
        {
            const string azureStorageDefaultConnectionString = "AzureStorageDefaultConnectionString";
            const string defaultAzureStorageContainer = "DefaultAzureStorageContainer";

            string containerName = CloudConfigurationManager.GetSetting(defaultAzureStorageContainer);

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(azureStorageDefaultConnectionString));

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container;
        }
    }
}
