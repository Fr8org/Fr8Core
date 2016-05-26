//using System;
//using System.Diagnostics;
//using StructureMap;
//using Infrastructure.Communication;
//using NUnit.Framework;
//using UtilitiesTesting;
//using UtilitiesTesting.Fixtures;
//using Utilities.Configuration.Azure;
//using terminalAzure;

//namespace terminalAzure.Tests
//{
//    [TestFixture]
//    public class SqlTests : BaseTest
//    {
//        public const string TestServerUrlKey = "TestServerUrl";

//        public const string WriteSqlCommand = "writeSQL";

//        private FixtureData _fixtureData;
//        private TestDbHelper _helper;
//        private IDisposable _server;


//        [SetUp]
//        public override void SetUp()
//        {
//            base.SetUp();

//            _fixtureData = new FixtureData(ObjectFactory.GetInstance<IUnitOfWork>());
//            _helper = new TestDbHelper();

//            // Check if table exists, then drop the test table.
//            // Create/recreate the test table.
//            using (var dbconn = _helper.CreateConnection())
//            {
//                dbconn.Open();

//                using (var tx = dbconn.BeginTransaction())
//                {
//                    if (_helper.TableExists(tx, _fixtureData.TestCustomerTable1_Schema(), _fixtureData.TestCustomerTable1_Table()))
//                    {
//                        _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Drop());
//                    }

//                    _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Create());

//                    tx.Commit();
//                }
//            }

//            var url = CloudConfigurationManager.GetSetting(TestServerUrlKey);
//            _server = SelfHostFactory.CreateServer(url);
//        }

//        [TearDown]
//        public void Cleanup()
//        {
//            // Check if table exists, then drop the test table.
//            using (var dbconn = _helper.CreateConnection())
//            {
//                dbconn.Open();

//                using (var tx = dbconn.BeginTransaction())
//                {
//                    if (_helper.TableExists(tx, _fixtureData.TestCustomerTable1_Schema(), _fixtureData.TestCustomerTable1_Table()))
//                    {
//                        _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Drop());
//                    }

//                    tx.Commit();
//                }
//            }

//            _server.Dispose();
//        }

//        [Test, Ignore] //this needs to be adjusted to work with the mockdb
//        public void CallCommandWrite()
//        {
//            var baseUrl = CloudConfigurationManager.GetSetting(TestServerUrlKey);

//            // Sending http request.
//            var restCall = ObjectFactory.GetInstance<IRestfulServiceClient>();
//            restCall.BaseUri = new Uri(baseUrl);
            
//            // Composing json data.
//            var content = new
//            {
//                connectionString = _helper.GetConnectionString().Replace("\\", "\\\\"),
//                provider = "System.Data.SqlClient",
//                tables = new[] {_fixtureData.TestCustomerTable1_Content()}
//            };

//            // Getting http response.
//            var response = restCall.PostAsync(new Uri(WriteSqlCommand, UriKind.Relative), content).Result;
//            Debug.WriteLine(response);            

//            // Validating correct data in database.
//            using (var dbconn = _helper.CreateConnection())
//            {
//                dbconn.Open();

//                using (var tx = dbconn.BeginTransaction())
//                {
//                    using (var cmd = dbconn.CreateCommand())
//                    {
//                        cmd.Transaction = tx;
//                        cmd.CommandText = "SELECT [firstName], [lastName] FROM [Customers] ORDER BY [firstName]";

//                        using (var reader = cmd.ExecuteReader())
//                        {
//                            string firstName;
//                            string lastName;

//                            // Checking we can fetch the first row.
//                            Assert.IsTrue(reader.Read());
                            
//                            // Checking firstName column.
//                            firstName = reader.GetString(0);
//                            Assert.AreEqual(firstName, "John");

//                            // Checking lastName column.
//                            lastName = reader.GetString(1);
//                            Assert.AreEqual(lastName, "Smith");


//                            // Checking we can fetch the second row.
//                            Assert.IsTrue(reader.Read());

//                            // Checking firstName column.
//                            firstName = reader.GetString(0);
//                            Assert.AreEqual(firstName, "Sam");

//                            // Checking lastName column.
//                            lastName = reader.GetString(1);
//                            Assert.AreEqual(lastName, "Jones");


//                            // Checking that no other column was created.
//                            Assert.IsFalse(reader.Read());
//                        }
//                    }

//                    tx.Commit();
//                }
//            }
//        }
//    }
//}
