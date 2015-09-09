using System.Configuration;
using NUnit.Framework;
using pluginAzureSqlServer.Actions;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Axtions
{
    [TestFixture]
    public class WriteToSqlServerV1Tests : BaseTest
    {
        private const string PayloadData =
            "{'payload': {'Physician' : 'Johnson','CurrentMedicalCondition' : 'Marthambles'}}";

        private Write_To_Sql_Server_v1 _sqServerWriter;
        private string _connectionString;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sqServerWriter = new Write_To_Sql_Server_v1();
            _connectionString =
                ConfigurationManager.ConnectionStrings["HealthDB"].ConnectionString;
        }


        private string CreateConfigurationStore()
        {
            const string configurationStore =
                "{'configurationStore': [{'textField': {'value': 'connection_string'}}]";

            return configurationStore.Replace("connection_string", _connectionString);
        }
    }
}