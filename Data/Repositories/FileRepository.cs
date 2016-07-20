using System;
using System.Collections.Generic;
using System.IO;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace Data.Repositories
{
    public class FileRepository : GenericRepository<FileDO>, IFileRepository
    {
        public FileRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public string SaveRemoteFile(Stream curRemoteFile, string curFileName)
        {
            var blobContainer = GetDefaultBlobContainer();
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(curFileName);
            blockBlob.UploadFromStream(curRemoteFile);

            return blockBlob.Uri.AbsoluteUri;
        }

        public byte[] GetRemoteFile(string curBlobUrl)
        {
            CloudBlobClient curCloudBlobClient = GetDefaultBlobContainer().ServiceClient;
            var curBlob = new CloudBlockBlob(new Uri(curBlobUrl), curCloudBlobClient.Credentials);
            curBlob.FetchAttributes();

            byte[] content = new byte[curBlob.Properties.Length];
            curBlob.DownloadToByteArray(content, 0, options: curCloudBlobClient.DefaultRequestOptions);

            return content;
        }

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
