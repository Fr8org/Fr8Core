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

            _incidentReporter = ObjectFactory.GetInstance <IncidentReporter>();
        } 

        [Test]
        public void TestTerminalRunFaildReport()
        {
            _incidentReporter.SubscribeToAlerts();

            var terminalUrl = "localhost:1234";
            var activityDTO = "test_action"; 
            var errorMessage = "error_message";

            //var data = terminalUrl + "      " + activityDTO + " " + errorMessage;
            var data = terminalUrl + "  ActionId = [" + activityDTO + "] " + errorMessage;

            EventManager.TerminalRunFailed(terminalUrl, activityDTO, errorMessage, System.Guid.NewGuid().ToString());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {   
                var foundItem = uow.IncidentRepository.FindOne(item => item.Data == data);
                Assert.IsNotNull(foundItem);
            }
        }
    }
}
