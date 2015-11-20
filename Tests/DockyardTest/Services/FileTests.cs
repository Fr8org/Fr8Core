using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Repositories;
using Hub.Interfaces;
using Utilities.Configuration.Azure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("File")]
    public class FileTests : BaseTest
    {
        private FileStream _curBlobFile;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData(_uow);

            if (_curBlobFile != null)
            {
                _curBlobFile.Close();
                _curBlobFile.Dispose();
                _curBlobFile = null;
            }
        }
        /*
        [Test]
        public void File_Store_CanStoreFile()
        {
            //Arrange
            FileDO curFileDO;
            
            //Act
            WriteRemoteFile();
            curFileDO = _uow.FileRepository.GetByKey(0);

            //Assert
            Assert.IsNotNull(curFileDO, "File is not uploaded to Azure Storage. The Files table is empty.");
            Assert.IsNotNullOrEmpty(curFileDO.CloudStorageUrl, "File is not uploaded to Azure Storage. The Cloud URL is empty.");
            Assert.IsTrue(curFileDO.CloudStorageUrl.EndsWith("small_pdf_file.pdf"), "File is not uploaded to Azure Storage. The BLOB names are not equal");

            _uow.FileRepository.Remove(curFileDO);
            _uow.SaveChanges();
        }
        */
        [Test]
        [ExpectedException(typeof(StorageException))]
        public void File_Store_WithTimeOutZero_ShouldThrowTimeOutExpcetion()
        {
            //Arrange
            MakeFileRepositoryTimeOut();

            //Act
            try
            {
                WriteRemoteFile();
            }
            catch (StorageException storageException)
            {
                //Assert
                Assert.AreEqual("TimeoutException", storageException.InnerException.GetType().Name, "Not an expected exception");
                throw storageException;
            }
        }

        [Test]
        [ExpectedException(typeof(StorageException))]
        public void File_Store_WithInvalidLogin_ShouldThrowTimeOutExpcetion()
        {
            //Arrange
            MakeFileRepositoryInvalidLogin();

            //Act
            try
            {
                WriteRemoteFile();
            }
            catch (StorageException storageException)
            {
                //Assert
                Assert.AreEqual("The remote server returned an error: (403) Forbidden.", storageException.Message, "Not an expected exception");
                throw storageException;
            }
        }
        /*
        [Test]
        public void File_Retrieve_CanRetrieveFile()
        {
            var file = ObjectFactory.GetInstance<IFile>();

            //Arrange
            WriteRemoteFile();
            FileDO curFileDO = _uow.FileRepository.GetByKey(0);

            //Act
            var curFile = file.Retrieve(curFileDO);

            //Assert
            Assert.IsNotNull(curFile, "File is not uploaded. Read attempt is failed.");
            Assert.IsTrue(curFile.Length>0, "File is not retireved properly. Read attempty is failed.");

            _uow.FileRepository.Remove(curFileDO);
            _uow.SaveChanges();
        }
        */
        [Test]
        [ExpectedException(typeof(StorageException))]
        public void File_Retrieve_WithTimeOutZero_ShouldThrowTimeOutException()
        {
            //Arrange
            WriteRemoteFile();
            FileDO curFileDO = _uow.FileRepository.GetByKey(0);
            MakeFileRepositoryTimeOut();

            var file = ObjectFactory.GetInstance<IFile>();

            //Act
            try
            {
                var curFile = file.Retrieve(curFileDO);
            }
            catch (StorageException storageException)
            {
                //Assert
                Assert.AreEqual("TimeoutException", storageException.InnerException.GetType().Name, "Not an expected exception");
                throw storageException;
            }
        }

        [Test]

        [ExpectedException(typeof(StorageException))]
        public void File_Retrieve_WithoutTargetFile_ShouldThrow404NotFound()
        {
            var file = ObjectFactory.GetInstance<IFile>();

            //Arrange
            WriteRemoteFile();
            FileDO curFileDO = _uow.FileRepository.GetByKey(0);
            file.Delete(curFileDO);

            //Act
            try
            {
                var curFile = file.Retrieve(curFileDO);
            }
            catch (StorageException storageException)
            {
                //Assert
                Assert.AreEqual("The remote server returned an error: (404) Not Found.", storageException.Message, "Not an expected exception");
                throw storageException;
            }
        }
        /*
        [Test]

        public void File_Delete_CanDeleteFile()
        {
            var file = ObjectFactory.GetInstance<IFile>();

            //Arrange
            WriteRemoteFile();
            FileDO curFileDO = _uow.FileRepository.GetByKey(0);

            //Act
            bool isFileDeleted = file.Delete(curFileDO);

            //Assert
            Assert.IsTrue(isFileDeleted, "File is not deleted successfully");
            curFileDO = _uow.FileRepository.GetByKey(curFileDO.Id);
            Assert.IsNull(curFileDO, "Updating database about file delete is failed.");
        }
        */
        private void WriteRemoteFile()
        {
            var file = ObjectFactory.GetInstance<IFile>();

            FileDO curFileDO = _fixtureData.TestFile1();
            _curBlobFile = new FileStream(FixtureData.TestRealPdfFile1(), FileMode.Open);
            string curNameOfBlob = "small_pdf_file.pdf";

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                file.Store(uow, curFileDO, _curBlobFile, curNameOfBlob);
            }
        }

        private void MakeFileRepositoryTimeOut()
        {
            var testCloudManager = new TestCloudFileManager();
            testCloudManager.SetTimeOutRequired();

            ObjectFactory.Container.Inject(typeof(CloudFileManager), testCloudManager);
        }

        private void MakeFileRepositoryInvalidLogin()
        {
            var testCloudManager = new TestCloudFileManager();
            testCloudManager.SetInvalidLoginRequired();

            ObjectFactory.Container.Inject(typeof(CloudFileManager), testCloudManager);
        }
    }

    public class TestCloudFileManager : CloudFileManager
    {
        private bool _isTimeOutRequired;
        private bool _isInvalidLoginRequired;

        public void SetTimeOutRequired()
        {
            _isTimeOutRequired = true;
        }

        public void SetInvalidLoginRequired()
        {
            _isInvalidLoginRequired = true;
        }

        protected override CloudBlobContainer GetDefaultBlobContainer()
        {
            //if no customization required, just return the base version of the BLOB container
            if (!_isTimeOutRequired && !_isInvalidLoginRequired)
            {
                return base.GetDefaultBlobContainer();
            }

            const string azureStorageDefaultConnectionString = "AzureStorageDefaultConnectionString";
            const string defaultAzureStorageContainer = "DefaultAzureStorageContainer";

            string containerName = CloudConfigurationManager.GetSetting(defaultAzureStorageContainer);

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(azureStorageDefaultConnectionString));

            if (_isInvalidLoginRequired)
            {
                storageAccount =
                    CloudStorageAccount.Parse(
                        "DefaultEndpointsProtocol=https;AccountName=yardstore1;AccountKey=SomeInvalidValue");
            }

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            if (_isTimeOutRequired)
            {
                var curBlobRequestOptions = new BlobRequestOptions();
                curBlobRequestOptions.ServerTimeout = TimeSpan.Zero;
                curBlobRequestOptions.MaximumExecutionTime = TimeSpan.Zero;
                blobClient.DefaultRequestOptions = curBlobRequestOptions;
            }

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container;
        }
    }
}
