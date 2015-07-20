using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Http.SelfHost;
using NUnit.Framework;
using pluginAzureSqlServer;

namespace pluginAzureSqlServerTests
{
    [TestFixture]
    public class CanSaveDataToSql
    {
        public const string WriteSqlUrlKey = "AzureSQLWriteCommandUrl";
        public const string TestServerUrlKey = "TestServerUrl";

        private TestDbHelper _helper;
        private HttpSelfHostServer _server;


        [SetUp]
        public void Init()
        {
            _helper = new TestDbHelper();
            using (var dbconn = _helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    if (_helper.CustomersTableExists(tx))
                    {
                        _helper.DropCustomersTable(tx);
                    }

                    _helper.CreateCustomersTable(tx);

                    tx.Commit();
                }
            }

            var url = ConfigurationManager.AppSettings[TestServerUrlKey];
            _server = SelfHostFactory.CreateServer(url);
            _server.OpenAsync().Wait();
        }

        [TearDown]
        public void Cleanup()
        {
            using (var dbconn = _helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    if (_helper.CustomersTableExists(tx))
                    {
                        _helper.DropCustomersTable(tx);
                    }

                    tx.Commit();
                }
            }

            _server.CloseAsync().Wait();
            _server.Dispose();
        }

        [Test]
        public void CallCommandWrite()
        {
            var url = ConfigurationManager.AppSettings[WriteSqlUrlKey];

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var writer = new StreamWriter(httpRequest.GetRequestStream()))
            {
                string json = string.Format(
                    @"{{
	                    ""connectionString"": ""{0}"",
	                    ""provider"": ""System.Data.SqlClient"",
	                    ""tables"": [ 
		                    {{
			                    ""Customers"": [
				                    {{
					                    ""firstName"": ""John"",
					                    ""lastName"": ""Smith""
				                    }},
				                    {{
					                    ""firstName"": ""Sam"", 
					                    ""lastName"": ""Jones""
				                    }},
			                    ]
		                    }}
	                    ]
                    }}",
                    _helper.GetConnectionString().Replace("\\", "\\\\")
                );

                writer.Write(json);
                writer.Flush();
                writer.Close();
            }

            var httpResponse = httpRequest.GetResponse();
            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var response = reader.ReadToEnd();
                System.Diagnostics.Debug.WriteLine(response);
            }

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

                            Assert.IsTrue(reader.Read());
                            
                            firstName = reader.GetString(0);
                            Assert.AreEqual(firstName, "John");

                            lastName = reader.GetString(1);
                            Assert.AreEqual(lastName, "Smith");


                            Assert.IsTrue(reader.Read());

                            firstName = reader.GetString(0);
                            Assert.AreEqual(firstName, "Sam");

                            lastName = reader.GetString(1);
                            Assert.AreEqual(lastName, "Jones");


                            Assert.IsFalse(reader.Read());
                        }
                    }

                    tx.Commit();
                }
            }
        }
    }
}
