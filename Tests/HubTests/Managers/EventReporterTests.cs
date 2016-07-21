using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Hub.Interfaces;
using Hub.Managers;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;

namespace HubTests.Managers
{
    [TestFixture]
    [Category("EventReporter")]
    public class EventReporterTests:BaseTest
    {
        private EventReporter _eventReporter;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _eventReporter = ObjectFactory.GetInstance<EventReporter>();
        }

        [Test]
        public void Should_trim_long_Data_in_historyItemDO_when_composing_string()
        {
            var historyItem = Fr8.Testing.Unit.Fixtures.FixtureData.TestFactDO();
            historyItem.Data = new String('s',257);

            //act
            var message1 = _eventReporter.ComposeOutputString(historyItem);
            historyItem.Data = new String('s', 400);
            var message2 = _eventReporter.ComposeOutputString(historyItem);

            Assert.AreEqual(message1, message2);
        }

        [Test]
        public void Should_take_null_Data_and_historyItemDO_when_composing_string()
        {
            var message1 = _eventReporter.ComposeOutputString(null);

            var historyItem = Fr8.Testing.Unit.Fixtures.FixtureData.TestFactDO();
            historyItem.Data = null;
            historyItem.CreatedBy = null;

            var message2 = _eventReporter.ComposeOutputString(historyItem);

            Assert.IsNotEmpty(message1);
            Assert.IsNotEmpty(message2);
        }

        [Test]
        public void Should_replace_Fr8UserId_with_CreatedById_for_FactDO()
        {
            var factDO = Fr8.Testing.Unit.Fixtures.FixtureData.TestFactDO();
            var id = "1234";
            factDO.Fr8UserId = null;
            factDO.CreatedByID = id;

            var message1 = _eventReporter.ComposeOutputString(factDO);

            factDO.Fr8UserId = "";

            var message2 = _eventReporter.ComposeOutputString(factDO);
            
            Assert.IsTrue(message1.Contains(id));
            Assert.IsTrue(message2.Contains(id));

        }
    }
}
