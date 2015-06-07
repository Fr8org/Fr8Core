using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.CalDAV;
using KwasantCore.Services;
using KwasantICS.DDay.iCal;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Assert = NUnit.Framework.Assert;

namespace KwasantTest.Managers
{
    [TestFixture]
    public class CalendarSyncManagerTest : BaseTest
    {
        private CalendarSyncManager _calendarSyncManager;
        private List<iCalendar> _remoteCalendarEvents;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _remoteCalendarEvents = new List<iCalendar>();
            CalendarSyncManager.DisableAutoSynchronization = true;
            
            var clientMock = new Mock<ICalDAVClient>();
            clientMock.Setup(c =>
                             c.GetEventsAsync(
                                 It.IsAny<IRemoteCalendarLinkDO>(),
                                 It.IsAny<DateTimeOffset>(),
                                 It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(_remoteCalendarEvents);
            clientMock.Setup(c =>
                             c.GetCalendarsAsync(It.IsAny<IRemoteCalendarAuthDataDO>()))
                .ReturnsAsync(new Dictionary<string, string>() { { "url", "name" } });
            clientMock.Setup(c =>
                              c.CreateEventAsync(It.IsAny<IRemoteCalendarLinkDO>(),
                                                 It.IsAny<iCalendar>()))
                .Returns<IRemoteCalendarLinkDO, iCalendar>((calendarLink, iCalEvent) =>
                             {
                                 _remoteCalendarEvents.Add(iCalEvent); 
                                 return Task.Delay(0);
                             });

            var clientFactoryMock = new Mock<ICalDAVClientFactory>();
            clientFactoryMock.Setup(f => f.Create(It.IsAny<IRemoteCalendarAuthDataDO>())).Returns(clientMock.Object);
            ObjectFactory.Configure(expression => expression.For<ICalDAVClientFactory>().Use(clientFactoryMock.Object));

            _calendarSyncManager = ObjectFactory.GetInstance<CalendarSyncManager>();
        }

        private void AssertEventsAreEqual(EventDO expectedEvent, EventDO actualEvent)
        {
            Assert.AreEqual(expectedEvent.Summary, actualEvent.Summary, "Mock event and generated one are not equal (Summary).");
            Assert.AreEqual(expectedEvent.Description, actualEvent.Description, "Mock event and generated one are not equal (Description).");
            Assert.AreEqual(expectedEvent.StartDate, actualEvent.StartDate, "Mock event and generated one are not equal (StartDate).");
            Assert.AreEqual(expectedEvent.EndDate, actualEvent.EndDate, "Mock event and generated one are not equal (EndDate).");
            Assert.AreEqual(expectedEvent.Location, actualEvent.Location, "Mock event and generated one are not equal (Location).");
        }

        [Test]
        [Category("CalendarSyncManager")]
        public async void CanAddToLocalCalendar()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curUser = fixture.TestUser1();
                var curProvider = fixture.TestRemoteCalendarProvider();
                var curAuthData = fixture.TestRemoteCalendarAuthData(curProvider, curUser);
                curUser.RemoteCalendarAuthData.Add(curAuthData);

                // SETUP
                var curEvent = fixture.TestEvent2();
                var iCalEvent = Event.GenerateICSCalendarStructure(curEvent);
                _remoteCalendarEvents.Add(iCalEvent);

                uow.SaveChanges();
                // EXECUTE
                Assert.AreEqual(1, _remoteCalendarEvents.Count,
                    "One event must be in the remote storage before synchronization.");
                Assert.AreEqual(0, uow.EventRepository.GetAll().Count(e => e.EventStatus != EventState.Deleted),
                    "No events must be in the repository before synchronization.");
                
                await _calendarSyncManager.SyncNowAsync(uow, curUser);
                uow.SaveChanges();

                // VERIFY
                var events = uow.EventRepository.GetAll().Where(e => e.EventStatus != EventState.Deleted).ToArray();
                Assert.AreEqual(1, events.Length, "One event must be in the repository after synchronization.");
                var newEvent = events[0];
                AssertEventsAreEqual(curEvent, newEvent);
            }
        }

        [Test]
        [Category("CalendarSyncManager")]
        public async void CanAddToRemoteCalendar()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curUser = fixture.TestUser1();
                var curProvider = fixture.TestRemoteCalendarProvider();
                var curAuthData = fixture.TestRemoteCalendarAuthData(curProvider, curUser);
                curUser.RemoteCalendarAuthData.Add(curAuthData);
                
                // SETUP
                var curEvent = fixture.TestEvent2();
                curEvent.SyncStatus = EventSyncState.SyncWithExternal;
                var curCalendarLink = fixture.TestRemoteCalendarLink(curProvider, curUser);
                curCalendarLink.LocalCalendar.Events.Add(curEvent);
                uow.RemoteCalendarLinkRepository.Add(curCalendarLink);
                uow.SaveChanges();

                // EXECUTE
                Assert.AreEqual(1, uow.EventRepository.GetAll().Count(e => e.EventStatus != EventState.Deleted),
                    "One event must be in the repository before synchronization.");
                Assert.AreEqual(0, _remoteCalendarEvents.Count,
                    "No events must be in the remote storage before synchronization.");
                await _calendarSyncManager.SyncNowAsync(uow, curUser);
                uow.SaveChanges();

                // VERIFY
                Assert.AreEqual(1, _remoteCalendarEvents.Count,
                    "One event must be in the remote storage after synchronization.");
                var newEvent = Event.CreateEventFromICSCalendar(uow, _remoteCalendarEvents[0]);
                AssertEventsAreEqual(curEvent, newEvent);
            }
        }

        [Test]
        [Category("CalendarSyncManager")]
        public async void CanRemoveFromLocalCalendar()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curUser = fixture.TestUser1();
                var curProvider = fixture.TestRemoteCalendarProvider();
                var curAuthData = fixture.TestRemoteCalendarAuthData(curProvider, curUser);
                curUser.RemoteCalendarAuthData.Add(curAuthData);
                
                // SETUP
                var curEvent = fixture.TestEvent2();
                curEvent.CreateDate = DateTimeOffset.UtcNow - TimeSpan.FromDays(1);
                curEvent.SyncStatus = EventSyncState.SyncWithExternal;
                uow.EventRepository.Add(curEvent);
                var curCalendarLink = fixture.TestRemoteCalendarLink(curProvider, curUser);
                curCalendarLink.LocalCalendar.Events.Add(curEvent);
                curCalendarLink.DateSynchronized = DateTimeOffset.UtcNow - TimeSpan.FromHours(1);
                uow.RemoteCalendarLinkRepository.Add(curCalendarLink);
                uow.SaveChanges();

                // EXECUTE
                Assert.AreEqual(1, uow.EventRepository.GetAll().Count(e => e.EventStatus != EventState.Deleted),
                    "One event must be in the repository before synchronization.");
                Assert.AreEqual(0, _remoteCalendarEvents.Count,
                    "No events must be in the remote storage before synchronization.");
                await _calendarSyncManager.SyncNowAsync(uow, curUser);
                uow.SaveChanges();

                // VERIFY
                Assert.AreEqual(0, uow.EventRepository.GetAll().Count(e => e.EventStatus != EventState.Deleted),
                    "No events must be in the repository after synchronization.");
                Assert.AreEqual(0, _remoteCalendarEvents.Count,
                    "No events must be in the remote storage after synchronization.");
            }
        }

        [Test]
        [Category("CalendarSyncManager")]
        public async void CanMergeCalendars()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curUser = fixture.TestUser1();
                var curProvider = fixture.TestRemoteCalendarProvider();
                var curAuthData = fixture.TestRemoteCalendarAuthData(curProvider, curUser);
                curUser.RemoteCalendarAuthData.Add(curAuthData);
                
                // SETUP
                var curLocalEvent = fixture.TestEvent2();
                curLocalEvent.SyncStatus = EventSyncState.SyncWithExternal;
                uow.EventRepository.Add(curLocalEvent);
                var curCalendarLink = fixture.TestRemoteCalendarLink(curProvider, curUser);
                curCalendarLink.LocalCalendar.Events.Add(curLocalEvent);
                uow.RemoteCalendarLinkRepository.Add(curCalendarLink);
                uow.SaveChanges();

                var curRemoteEvent = fixture.TestEvent2();
                curRemoteEvent.CreateDate = curLocalEvent.CreateDate;
                curRemoteEvent.StartDate = curLocalEvent.StartDate;
                curRemoteEvent.EndDate = curLocalEvent.EndDate;
                curRemoteEvent.Location = "changed location";
                curRemoteEvent.Description = "changed description";
                var iCalEvent = Event.GenerateICSCalendarStructure(curRemoteEvent);
                _remoteCalendarEvents.Add(iCalEvent);

                // EXECUTE
                Assert.AreEqual(1, uow.EventRepository.GetAll().Count(e => e.EventStatus != EventState.Deleted),
                    "One event must be in the repository before synchronization.");
                Assert.AreEqual(1, _remoteCalendarEvents.Count,
                    "One event must be in the remote storage before synchronization.");
                await _calendarSyncManager.SyncNowAsync(uow, curUser);
                uow.SaveChanges();

                // VERIFY
                var localEvents = uow.EventRepository.GetAll().Where(e => e.EventStatus != EventState.Deleted).ToArray();
                Assert.AreEqual(1, localEvents.Length, "One event must be in the repository after synchronization.");
                Assert.AreEqual(1, _remoteCalendarEvents.Count,
                    "One event must be in the remote storage after synchronization.");
                var newLocalEvent = localEvents[0];
                var newRemoteEvent = Event.CreateEventFromICSCalendar(uow, _remoteCalendarEvents[0]);
                AssertEventsAreEqual(newLocalEvent, newRemoteEvent);
                AssertEventsAreEqual(curRemoteEvent, newRemoteEvent);
            }
        }
    }
}
