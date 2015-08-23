using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Interfaces;
using Core.Services;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using StructureMap;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Microsoft.SqlServer.Management.Common;
using Server = Microsoft.SqlServer.Management.Smo.Server;

namespace DockyardTest.Integration
{
    [TestFixture]
    public class RunTimeTests : BaseTest
    {

        [Test]
        [Category("IntegrationTests")]
        public async void ITest_CanProcessHealthDemo()
        {
            string email;
            string id;
            // SETUP
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var healthProcessTemplateDO = FixtureData.TestProcessTemplateHealthDemo();

                //flush database, creating it if necessary.
                string connectionString = FixtureData.TestConnectionString1().Value;
                string script = FixtureData.TestSqlScript_healthdemo();

                //SqlConnection conn = new SqlConnection(healthdemo_ConnectionString);
                //Server sqlServer = new Server(new ServerConnection(conn));
                //sqlServer.ConnectionContext.ExecuteNonQuery(script);





                //string connectionString = "Server=tcp:XXXXX.database.windows.net;Database=XXXXXX;User ID=XXXXXX;Password=XXXXX;Trusted_Connection=False;Encrypt=True;trustservercertificate=true";
                SqlConnection connection = new SqlConnection(connectionString);
                // do not explicitly open connection, it will be opened when Server is initialized
                // connection.Open();

                ServerConnection serverConnection = new ServerConnection(connection);
                Server server = new Server(serverConnection);

                // after this line, the default database will be switched to Master
                Database database = server.Databases["demodb_health"];

                // you can still use this database object and server connection to 
                // do certain things against this database, like adding database roles 
                // and users      
               
              

                // if you want to execute a script against this database, you have to open 
                // another connection and re-initiliaze the server object
                server.ConnectionContext.Disconnect();

                connection = new SqlConnection(connectionString);
                serverConnection = new ServerConnection(connection);
                server = new Server(serverConnection);
                server.ConnectionContext.ExecuteNonQuery("CREATE TABLE New (NewId int)");
            }

            // EXECUTE


            // VERIFY
            //Assert.AreEqual(id, userId);
            //Assert.IsTrue(result.Succeeded, string.Join(", ", result.Errors));

        }
    }
}
