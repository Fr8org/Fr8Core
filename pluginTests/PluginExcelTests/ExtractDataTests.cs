using System;
using System.Configuration;
using System.Diagnostics;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces;
using NUnit.Framework;
using pluginAzureSqlServer;
using StructureMap;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace pluginTests.PluginAzureSqlServerTests
{
    [TestFixture]
    public class ExtractDataTests : BaseTest
    {
        public const string ExcelTestServerUrl = "ExcelTestServerUrl";

        public const string filesCommand = "files";

        private FixtureData _fixtureData;
        private TestDbHelper _helper;
        private IDisposable _server;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _fixtureData = new FixtureData(ObjectFactory.GetInstance<IUnitOfWork>());
            //_helper = new TestDbHelper();

            //// Check if table exists, then drop the test table.
            //// Create/recreate the test table.
            //using (var dbconn = _helper.CreateConnection())
            //{
            //    dbconn.Open();

            //    using (var tx = dbconn.BeginTransaction())
            //    {
            //        if (_helper.TableExists(tx, _fixtureData.TestCustomerTable1_Schema(), _fixtureData.TestCustomerTable1_Table()))
            //        {
            //            _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Drop());
            //        }

            //        _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Create());

            //        tx.Commit();
            //    }
            //}

            var url = ConfigurationManager.AppSettings[ExcelTestServerUrl];
            _server = SelfHostFactory.CreateServer(url);
        }

        [TearDown]
        public void Cleanup()
        {
            //// Check if table exists, then drop the test table.
            //using (var dbconn = _helper.CreateConnection())
            //{
            //    dbconn.Open();

            //    using (var tx = dbconn.BeginTransaction())
            //    {
            //        if (_helper.TableExists(tx, _fixtureData.TestCustomerTable1_Schema(), _fixtureData.TestCustomerTable1_Table()))
            //        {
            //            _helper.ExecuteSql(tx, _fixtureData.TestCustomerTable1_Drop());
            //        }

            //        tx.Commit();
            //    }
            //}

            _server.Dispose();
        }

        [Test]
        public void CallExtractData_Execute()
        {
            var baseUrl = ConfigurationManager.AppSettings[ExcelTestServerUrl];

            // Sending http request.
            var restCall = ObjectFactory.GetInstance<IRestfulServiceClient>();
            restCall.BaseUri = new Uri(baseUrl);
            
            // Composing json data.
            var content = new
            {
                connectionString = _helper.GetConnectionString().Replace("\\", "\\\\"),
                provider = "System.Data.SqlClient",
                tables = new[] {_fixtureData.TestCustomerTable1_Content()}
            };

            // Getting http response.
            var response = restCall.PostAsync(new Uri(filesCommand, UriKind.Relative), content).Result;
            Debug.WriteLine(response);            

            
        }
    }
}
