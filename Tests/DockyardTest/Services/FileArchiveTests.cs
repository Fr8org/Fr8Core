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
    public class FileArchiveTests : BaseTest
    {
        private IFileArchive _fileArchive;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _fileArchive = ObjectFactory.GetInstance<IFileArchive>();
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData(_uow);
        }

        [Test]
        public void FileArchive_WriteFile_CanWriteFile()
        {
            //Arrange
            FileDO curFileDo;
            
            //Act
            WriteRemoteFile();
            curFileDo = _uow.FileRepository.GetByKey(0);

            //Assert
            Assert.IsNotNull(curFileDo, "File is not uploaded to Azure Storage. The Files table is empty.");
            Assert.IsNotNullOrEmpty(curFileDo.CloudStorageUrl, "File is not uploaded to Azure Storage. The Cloud URL is empty.");
            Assert.IsTrue(curFileDo.CloudStorageUrl.EndsWith("small_pdf_file.pdf"), "File is not uploaded to Azure Storage. The BLOB names are not equal");

            _uow.FileRepository.Remove(curFileDo);
            _uow.SaveChanges();
        }

        [Test]
        public void FileArchive_ReadFile_CanReadFile()
        {
            //Arrange
            WriteRemoteFile();
            FileDO curFileDo = _uow.FileRepository.GetByKey(0);

            //Act
            var curFile = _fileArchive.ReadFile(curFileDo);

            //Assert
            Assert.IsNotNull(curFile, "File is not uploaded. Read attempt is failed.");
            Assert.IsTrue(curFile.Length>0, "File is not retireved properly. Read attempty is failed.");

            _uow.FileRepository.Remove(curFileDo);
            _uow.SaveChanges();
        }

        [Test]
        public void FileArchive_DeleteFile_CanDeleteFile()
        {
            //Arrange
            WriteRemoteFile();
            FileDO curFileDo = _uow.FileRepository.GetByKey(0);

            //Act
            bool isFileDeleted = _fileArchive.DeleteFile(curFileDo);

            //Assert
            Assert.IsTrue(isFileDeleted);
            curFileDo = _uow.FileRepository.GetByKey(curFileDo.Id);
            Assert.IsNull(curFileDo);
        }

        private void WriteRemoteFile()
        {
            FileDO curFileDo = _fixtureData.TestFile1();
            FileStream curBlobFile = new FileStream(FixtureData.TestRealPdfFile1(), FileMode.Open);
            string curNameOfBlob = "small_pdf_file.pdf";

            _fileArchive.WriteFile(curFileDo, curBlobFile, curNameOfBlob);

            curBlobFile.Close();
        }
    }
}
