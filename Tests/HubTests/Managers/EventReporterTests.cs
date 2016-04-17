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
    [Category("EventReporter")]
    public class EventReporterTests : BaseTest
    {
        private EventReporter _eventReporter;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _eventReporter = ObjectFactory.GetInstance<EventReporter>();
        }

        [Test]
        public void Must_have_right_output_string_format()
        {

            var factItem = UtilitiesTesting.Fixtures.FixtureData.TestFactDO();
            var incidentTitem = UtilitiesTesting.Fixtures.FixtureData.TestIncidentDO();

            var factLogString = _eventReporter.ComposeOutputString(factItem);
            var incidentLogString = _eventReporter.ComposeOutputString(incidentTitem);

            Assert.IsTrue(factLogString.Contains("Fact"));
            Assert.IsTrue(incidentLogString.Contains("Incident"));

            //since it just test, we cant say sure will there be passed appropriate Fr8UserId or not,
            //check that it has it`s field at least
            Assert.IsTrue(factLogString.Contains("Fr8User"));
            Assert.IsTrue(incidentLogString.Contains("Fr8User"));

            Assert.IsTrue(factLogString.Contains("Data"));
            Assert.IsTrue(incidentLogString.Contains("Data"));

            Assert.IsTrue(factLogString.Contains("ObjectId"));
            Assert.IsTrue(incidentLogString.Contains("ObjectId"));
        }
    }
}
