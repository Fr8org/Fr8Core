using System.IO;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("FileArchive")]
    public class FileTests : BaseTest
    {
        private IFile _file;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _file = ObjectFactory.GetInstance<IFile>();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData(_uow);
        }

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

        [Test]
        public void File_Retrieve_CanRetrieveFile()
        {
            //Arrange
            WriteRemoteFile();
            FileDO curFileDO = _uow.FileRepository.GetByKey(0);

            //Act
            var curFile = _file.Retrieve(curFileDO);

            //Assert
            Assert.IsNotNull(curFile, "File is not uploaded. Read attempt is failed.");
            Assert.IsTrue(curFile.Length>0, "File is not retireved properly. Read attempty is failed.");

            _uow.FileRepository.Remove(curFileDO);
            _uow.SaveChanges();
        }

        [Test]
        public void File_Delete_CanDeleteFile()
        {
            //Arrange
            WriteRemoteFile();
            FileDO curFileDO = _uow.FileRepository.GetByKey(0);

            //Act
            bool isFileDeleted = _file.Delete(curFileDO);

            //Assert
            Assert.IsTrue(isFileDeleted);
            curFileDO = _uow.FileRepository.GetByKey(curFileDO.Id);
            Assert.IsNull(curFileDO);
        }

        private void WriteRemoteFile()
        {
            FileDO curFileDO = _fixtureData.TestFile1();
            FileStream curBlobFile = new FileStream(FixtureData.TestRealPdfFile1(), FileMode.Open);
            string curNameOfBlob = "small_pdf_file.pdf";

            _file.Store(curFileDO, curBlobFile, curNameOfBlob);

            curBlobFile.Close();
        }
    }
}
