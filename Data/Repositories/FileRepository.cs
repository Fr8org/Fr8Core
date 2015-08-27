using System;
using System.IO;
using Data.Entities;
using Data.Interfaces;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Data.Repositories
{
    public class FileRepository : GenericRepository<FileDO>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow) : base(uow)
        {
            
        }

        public string SaveRemoteFile(FileStream curRemoteFile, string curFileName)
        {
            var blobContainer = GetDefaultBlobContainer();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(curFileName);
            blockBlob.UploadFromStream(curRemoteFile);

            return blockBlob.Uri.AbsoluteUri;
        }

        public byte[] GetRemoteFile(string curBlobUrl)
        {
            var curBlob = new CloudBlockBlob(new Uri(curBlobUrl), GetDefaultBlobContainer().ServiceClient.Credentials);
            curBlob.FetchAttributes();

            byte[] content = new byte[curBlob.Properties.Length];
            curBlob.DownloadToByteArray(content, 0);
            
            return content;
        }

        public bool DeleteRemoteFile(string curBlobUrl)
        {
            var curBlob = new CloudBlockBlob(new Uri(curBlobUrl), GetDefaultBlobContainer().ServiceClient.Credentials);
            return curBlob.DeleteIfExists();
        }

        private CloudBlobContainer GetDefaultBlobContainer()
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
