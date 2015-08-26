using Core.Interfaces;
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
            
        }

        [Test]
        public void FileArchive_ReadFile_CanReadFile()
        {
            
        }

        [Test]
        public void FileArchive_DeleteFile_CanDeleteFile()
        {
            
        }
    }
}
