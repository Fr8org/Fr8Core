using System;
using System.Configuration;
using System.IO;
using System.Net;
using NUnit.Framework;

namespace pluginAzureSqlServerTests
{
    [TestFixture]
    public class CanSaveDataToSql
    {
        public const string WriteSqlUrlKey = "AzureSQLWriteCommandUrl";


        [SetUp]
        public void Init()
        {
            var helper = new TestDbHelper();
            using (var dbconn = helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    if (helper.CustomersTableExists(tx))
                    {
                        helper.DropCustomersTable(tx);
                    }

                    helper.CreateCustomersTable(tx);

                    tx.Commit();
                }
            }
        }

        [TearDown]
        public void Cleanup()
        {
            var helper = new TestDbHelper();
            using (var dbconn = helper.CreateConnection())
            {
                dbconn.Open();

                using (var tx = dbconn.BeginTransaction())
                {
                    if (helper.CustomersTableExists(tx))
                    {
                        helper.DropCustomersTable(tx);
                    }

                    tx.Commit();
                }
            }
        }

        [Test]
        public void CallCommandWrite()
        {
            var helper = new TestDbHelper();
            var url = ConfigurationManager.AppSettings[WriteSqlUrlKey];

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                string json = string.Format(
                    @"{{
	                    ""connectionString"": ""Data Source={0}"",
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
                    helper.GetConnectionString().Replace("\\", "\\\\")
                );

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            httpRequest.GetResponse();

            using (var dbconn = helper.CreateConnection())
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
