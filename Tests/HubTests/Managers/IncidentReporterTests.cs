using Data.Infrastructure;
using Data.Interfaces;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

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
            var additionalData = "test_action"; 
            var errorMessage = "error_message";

            //var data = terminalUrl + "      " + activityDTO + " " + errorMessage;
            var data = terminalUrl + " [ " + additionalData + " ] " + errorMessage;

            EventManager.TerminalRunFailed(terminalUrl, additionalData, errorMessage, System.Guid.NewGuid().ToString());

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {   
                var foundItem = uow.IncidentRepository.FindOne(item => item.Data == data);
                Assert.IsNotNull(foundItem);
            }
        }
    }
}
