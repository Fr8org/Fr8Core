using System;
using System.Configuration;
using System.IO;
using System.Net;
using NUnit.Framework;
using StructureMap;
using Core.ExternalServices.REST;
using Data.Interfaces;
using TestCommons;
using TestCommons.Fixtures;
using pluginAzureSqlServer;


namespace pluginAzureSqlServerTests
{
    [TestFixture]
    public class SqlTests : BaseTest
    {
        public const string TestServerUrlKey = "TestServerUrl";

        public const string WriteSqlCommand = "writeSQL";

        private FixtureData _fixtureData;
        private TestDbHelper _helper;
        private IDisposable _server;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _fixtureData = new FixtureData(ObjectFactory.GetInstance<IUnitOfWork>());
            _helper = new TestDbHelper();

            // Check if table exists, then drop the test table.
            // Create/recreate the test table.
            using (var dbconn = _helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    if (_helper.TableExists(tx, _fixtureData.TestCustomerTable1_Schema(), _fixtureData.TestCustomerTable1_Table()))
                    {
                        _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Drop());
                    }

                    _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Create());

                    tx.Commit();
                }
            }

            var url = ConfigurationManager.AppSettings[TestServerUrlKey];
            _server = SelfHostFactory.CreateServer(url);
        }

        [TearDown]
        public void Cleanup()
        {
            // Check if table exists, then drop the test table.
            using (var dbconn = _helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    if (_helper.TableExists(tx, _fixtureData.TestCustomerTable1_Schema(), _fixtureData.TestCustomerTable1_Table()))
                    {
                        _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Drop());
                    }

                    tx.Commit();
                }
            }

            _server.Dispose();
        }

        [Test]
        public void CallCommandWrite()
        {
            var baseUrl = ConfigurationManager.AppSettings[TestServerUrlKey];

            // Sending http request.
            var restCall = ObjectFactory.GetInstance<IRestfullCall>();
            restCall.Initialize(baseUrl, WriteSqlCommand, Utilities.Method.POST);

            // Composing json data.
            var json = string.Format(
                @"{{
	                ""connectionString"": ""{0}"",
	                ""provider"": ""System.Data.SqlClient"",
	                ""tables"": [ 
		                {{
			                {1}
		                }}
	                ]
                }}",
                // We need to escape single back-slash to form a valid json string.
                _helper.GetConnectionString().Replace("\\", "\\\\"),
                // Place fixture data into json.
                _fixtureData.TestCustomerTable1_Json()
            );

            restCall.AddBody(json, "application/json");

            // Getting http response.
            var response = restCall.Execute();
            System.Diagnostics.Debug.WriteLine(response);            

            // Validating correct data in database.
            using (var dbconn = _helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    using (var cmd = dbconn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "SELECT [firstName], [lastName] FROM [Customers] ORDER BY [firstName]";

                        using (var reader = cmd.ExecuteReader())
                        {
                            string firstName;
                            string lastName;

                            // Checking we can fetch the first row.
                            Assert.IsTrue(reader.Read());
                            
                            // Checking firstName column.
                            firstName = reader.GetString(0);
                            Assert.AreEqual(firstName, "John");

                            // Checking lastName column.
                            lastName = reader.GetString(1);
                            Assert.AreEqual(lastName, "Smith");


                            // Checking we can fetch the second row.
                            Assert.IsTrue(reader.Read());

                            // Checking firstName column.
                            firstName = reader.GetString(0);
                            Assert.AreEqual(firstName, "Sam");

                            // Checking lastName column.
                            lastName = reader.GetString(1);
                            Assert.AreEqual(lastName, "Jones");


                            // Checking that no other column was created.
                            Assert.IsFalse(reader.Read());
                        }
                    }

                    tx.Commit();
                }
            }
        }
    }
}
