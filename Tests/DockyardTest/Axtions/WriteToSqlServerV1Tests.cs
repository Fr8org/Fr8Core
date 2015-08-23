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

        [Test]
        [Category("Write_To_Sql_Server_v1.Execute")]
        public void WriteToSqlServerV1_CanWriteData()
        {
            var curActionData = FixtureData.TestAction1();
            curActionData.PayloadMappings = PayloadData;
            curActionData.ConfigurationSettings = CreateConfigurationSettings();

            _sqServerWriter.Process("execute", curActionData);
        }

        private string CreateConfigurationSettings()
        {
            const string configurationSettings =
                "{'configurationSettings': [{'textField': {'value': 'connection_string'}}]";

            return configurationSettings.Replace("connection_string", _connectionString);
        }
    }
}