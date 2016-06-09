using System;
using NUnit.Framework;
using StructureMap;
using System.IO;
using System.Linq;
using Data.Infrastructure;
using Data.Interfaces;
using Fr8.Testing.Unit;
using Microsoft.WindowsAzure;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using terminalUtilities.Excel;

namespace HubTests
{
    [TestFixture]
    [Category("ExcelUtils")]
    public class ExcelUtilsTests : BaseTest
    {
        private string _fakeExcelXlsPath;
        private string _fakeExcelXlsxPath;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _fakeExcelXlsPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xls";
            _fakeExcelXlsxPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xlss";
            File.WriteAllText(_fakeExcelXlsPath, "ASD");
            File.WriteAllText(_fakeExcelXlsxPath, "ASD");
        }
        [TearDown]
        public void Dispose()
        {
            File.Delete(_fakeExcelXlsPath);
            File.Delete(_fakeExcelXlsxPath);
        }
        [Test]
        public void ConvertToCsv_PathToExcelIsNull_ExpectedArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => ExcelUtils.ConvertToCsv(null, "C:\\1.csv"));

            Assert.AreEqual("pathToExcel", ex.ParamName);
        }
        [Test]
        public void ConvertToCsv_PathToExcelIsEmtpy_ExpectedArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => ExcelUtils.ConvertToCsv(string.Empty, "C:\\1.csv"));

            Assert.AreEqual("pathToExcel", ex.ParamName);
        }
        [Test]
        public void ConvertToCsv_PathToExcelDoesntExist_ExpectedFileNotFoundException()
        {
            string pathToExcel = "C:\\" + Guid.NewGuid() + ".xls";
            var ex = Assert.Throws<FileNotFoundException>(() => ExcelUtils.ConvertToCsv(pathToExcel, "C:\\1.csv"));

            Assert.AreEqual(pathToExcel, ex.FileName);
        }
        [Test]
        public void ConvertToCsv_PathToCsvIsNull_ExpectedArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => ExcelUtils.ConvertToCsv(_fakeExcelXlsPath, null));

            Assert.AreEqual("pathToCsv", ex.ParamName);
        }
        [Test]
        public void ConvertToCsv_PathToCsvIsEmpty_ExpectedArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => ExcelUtils.ConvertToCsv(_fakeExcelXlsPath, string.Empty));

            Assert.AreEqual("pathToCsv", ex.ParamName);
        }
        [Test]
        public void ConvertToCsv_PathToExcelIsNotExcelFile_ExpectedArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => ExcelUtils.ConvertToCsv("C:\\1.blablabla", "C:\\1.csv"));

            Assert.AreEqual("pathToExcel", ex.ParamName);
            bool doesContain = ex.Message.Contains("Expected '.xls' or '.xlsx'");
            Assert.AreEqual(true, doesContain, "Expected '.xls' or '.xlsx'");
        }
        [Test]
        public void Functional_ConvertToCsv_1ColumnXlsx_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\1Column.xlsx", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(1, columns.Length, "Expected only 1 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_2ColumnsXlsx_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\2Columns.xlsx", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(2, columns.Length, "Expected only 2 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                    Assert.AreEqual("Column2", columns[1], "Expected column 'Column2'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_3ColumnsXlsx_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\3Columns.xlsx", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(3, columns.Length, "Expected only 3 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                    Assert.AreEqual("Column2", columns[1], "Expected column 'Column2'");
                    Assert.AreEqual("Column3", columns[2], "Expected column 'Column3'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_10ColumnsXlsx_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\10Columns.xlsx", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(10, columns.Length, "Expected only 10 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                    Assert.AreEqual("Column2", columns[1], "Expected column 'Column2'");
                    Assert.AreEqual("Column3", columns[2], "Expected column 'Column3'");
                    Assert.AreEqual("Column4", columns[3], "Expected column 'Column4'");
                    Assert.AreEqual("Column5", columns[4], "Expected column 'Column5'");
                    Assert.AreEqual("Column6", columns[5], "Expected column 'Column6'");
                    Assert.AreEqual("Column7", columns[6], "Expected column 'Column7'");
                    Assert.AreEqual("Column8", columns[7], "Expected column 'Column8'");
                    Assert.AreEqual("Column9", columns[8], "Expected column 'Column9'");
                    Assert.AreEqual("Column10", columns[9], "Expected column 'Column10'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_1ColumnXls_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\1Column.xls", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(1, columns.Length, "Expected only 1 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_2ColumnsXls_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\2Columns.xls", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(2, columns.Length, "Expected only 2 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                    Assert.AreEqual("Column2", columns[1], "Expected column 'Column2'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_3ColumnsXls_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\3Columns.xls", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(3, columns.Length, "Expected only 3 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                    Assert.AreEqual("Column2", columns[1], "Expected column 'Column2'");
                    Assert.AreEqual("Column3", columns[2], "Expected column 'Column3'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test]
        public void Functional_ConvertToCsv_10ColumnsXls_ShouldBeOk()
        {
            string pathToCsv = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
            try
            {
                Assert.DoesNotThrow(() => ExcelUtils.ConvertToCsv(@"Tools\FileTools\TestFiles\10Columns.xls", pathToCsv));

                using (ICsvReader csvReader = new CsvReader(pathToCsv))
                {
                    var columns = csvReader.GetColumnHeaders();

                    Assert.AreEqual(10, columns.Length, "Expected only 10 column");
                    Assert.AreEqual("Column1", columns[0], "Expected column 'Column1'");
                    Assert.AreEqual("Column2", columns[1], "Expected column 'Column2'");
                    Assert.AreEqual("Column3", columns[2], "Expected column 'Column3'");
                    Assert.AreEqual("Column4", columns[3], "Expected column 'Column4'");
                    Assert.AreEqual("Column5", columns[4], "Expected column 'Column5'");
                    Assert.AreEqual("Column6", columns[5], "Expected column 'Column6'");
                    Assert.AreEqual("Column7", columns[6], "Expected column 'Column7'");
                    Assert.AreEqual("Column8", columns[7], "Expected column 'Column8'");
                    Assert.AreEqual("Column9", columns[8], "Expected column 'Column9'");
                    Assert.AreEqual("Column10", columns[9], "Expected column 'Column10'");
                }
            }
            finally
            {
                try { File.Delete(pathToCsv); }
                catch { }
            }
        }
        [Test] //this test requires internet connection
        public void GetColumnHeadersTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cloudFileManager = new MockedCloudFileManager();
                // https://yardstore1.blob.core.windows.net/default-container-dev/SampleFile1.xlsx
                var blobUrl = "https://yardstore1.blob.core.windows.net/default-container-dev/SampleFile1.xlsx";
                try
                {
                    var byteArray = cloudFileManager.GetRemoteFile(blobUrl);
                    var columns = ExcelUtils.GetColumnHeaders(byteArray, "xlsx");
                    Assert.IsNotNull(columns);
                    Assert.AreEqual(columns.Count(), 3);
                    Assert.AreEqual(columns[0], "FirstName");
                    Assert.AreEqual(columns[1], "LastName");
                    Assert.AreEqual(columns[2], "Email Address");
                }
                finally
                {
                }
            }
        }
        [Test]
        public void GetRowsTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var cloudFileManager = new MockedCloudFileManager();
                // https://yardstore1.blob.core.windows.net/default-container-dev/SampleFile1.xlsx
                var blobUrl = "https://yardstore1.blob.core.windows.net/default-container-dev/SampleFile1.xlsx";
                try
                {
                    var byteArray = cloudFileManager.GetRemoteFile(blobUrl);
                    var rows = ExcelUtils.GetTabularData(byteArray, "xlsx");
                    Assert.IsNotNull(rows);
                    Assert.AreEqual(rows.Count(), 3);
                    Assert.AreEqual(rows["1"].Count, 3);
                    Assert.AreEqual(rows["1"][0].Item1, "1");
                    Assert.AreEqual(rows["1"][0].Item2, "Alex");
                    Assert.AreEqual(rows["2"].Count, 3);
                    Assert.AreEqual(rows["3"].Count, 3);
                }
                finally
                {
                }
            }
        }
    }

    /// <summary>
    /// Had to write this class only for mocking out reading from CloudConfigurationManager as AppVeyor build mechanism is somehow failing to do so.
    /// </summary>
    public class MockedCloudFileManager : CloudFileManager
    {
        protected override CloudBlobContainer GetDefaultBlobContainer()
        {
            const string azureStorageDefaultConnectionString = "AzureStorageDefaultConnectionString";
            const string defaultAzureStorageContainer = "DefaultAzureStorageContainer";

            string containerName = "default-container-dev"; // CloudConfigurationManager.GetSetting(defaultAzureStorageContainer);
            
            //CloudStorageAccount storageAccount =
            //    CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(azureStorageDefaultConnectionString));

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=yardstore1;AccountKey=Or8iJLqkutxYCSKHiOo8iwSwyALCdFfR/RUTWSEZ9BPhLY4+L2QKVEean0bYSmVzCNSNSqBt2/zVA5HMgkwayg==");

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container;
        }
    }
}
