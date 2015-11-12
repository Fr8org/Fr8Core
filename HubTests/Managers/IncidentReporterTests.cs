using Data.Infrastructure;
using Data.Interfaces;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace HubTests.Managers
{
    [TestFixture]
    [Category("IncidentReporter")]
    public class IncidentReporterTests : BaseTest
    {
        private IncidentReporter _incidentReporter;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _incidentReporter = new IncidentReporter();
        } 

        [Test]
        public void TestTerminalRunFaildReport()
        {
            _incidentReporter.SubscribeToAlerts();

            var terminalUrl = "localhost:1234";
            var actionDTO = "test_action"; 
            var errorMessage = "error_message";

            var data = terminalUrl + "      " + actionDTO + " " + errorMessage;

            EventManager.TerminalRunFailed(terminalUrl, actionDTO, errorMessage);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var list = uow.IncidentRepository.GetAll();
                foreach (var item in list)
                {
                    System.Diagnostics.Trace.WriteLine(item.ToString());
                }
                var foundItem = uow.IncidentRepository.FindOne(item => item.Data == data);

                Assert.IsNotNull(foundItem);
            }
        }
    }
}
