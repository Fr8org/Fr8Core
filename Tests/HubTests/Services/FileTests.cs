using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;

using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Services
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
                throw;
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
                Assert.AreEqual(403, storageException.RequestInformation.HttpStatusCode, "Not an expected exception");
                throw;
            }
        }
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
                throw;
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
                Assert.AreEqual(404, storageException.RequestInformation.HttpStatusCode, "Not an expected exception");
                throw;
            }
        }
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
