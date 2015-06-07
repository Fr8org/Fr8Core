using System;
using System.Linq;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Controllers
{
    [TestFixture]
    public class EventControllerTests : BaseTest
    {
        //create real eventcontroller with real Attendee and Event (later should isolate these with mocks but need to test integration now)
        //create mock event with id 1 and save it to the mockdb
        //create eventvm with a new attendee string with an email, and with id 1 to match the event in the mockdb
        //call processconfirmedevent
        //should complete without an exception
        [Test, Category("EventController")]
        public void CanProcessConfirmedEvent()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var user = fixture.TestUser1();
                EventDO testEvent = fixture.TestEvent1();
                testEvent.BookingRequest = fixture.TestBookingRequest1();
                testEvent.CreatedBy = user;

                testEvent.EventStatus = EventState.ProposedTimeSlot;
                    //a convenient way to get DispatchInvitations to do nothing. 
                uow.EventRepository.Add(testEvent);
                uow.SaveChanges();

                EventController curEventController = new EventController();
                var curEventVM = new EventVM
                {
                    Id = testEvent.Id,
                    Summary = String.Empty
                };
                curEventVM.Attendees = "newattendee@kwasant.net";
                curEventController.ProcessChangedEvent(curEventVM, ConfirmationStatus.Confirmed, false);
            }

        }

        [Test]
        public void TestTimezonesCorrect()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var br = fixture.TestBookingRequest1();
                var cal = fixture.TestCalendar1();
                br.Calendars.Add(cal);
                cal.BookingRequests.Add(br);
                uow.CalendarRepository.Add(cal);

                const string createDateStr = @"Sat, 29 Nov 2014 07:00:00 +0700";
                br.CreateDate = DateTimeOffset.Parse(createDateStr);
                uow.BookingRequestRepository.Add(br);

                uow.SaveChanges();
                
                var ec = new EventController();
                ViewResult result = ec.New(br.Id, cal.Id, "2014-11-29T11:00:00z", "2014-11-29T11:30:00z") as ViewResult;
                var model = result.Model as EventVM;

                var createdEventID = model.Id;
                var eventDO = uow.EventRepository.GetByKey(createdEventID);

                Assert.AreEqual(new TimeSpan(7, 0, 0), eventDO.StartDate.Offset);
                Assert.AreEqual(11, eventDO.StartDate.Hour);
                Assert.AreEqual(0, eventDO.StartDate.Minute);
                Assert.AreEqual(new TimeSpan(7, 0, 0), eventDO.EndDate.Offset);
                Assert.AreEqual(11, eventDO.EndDate.Hour);
                Assert.AreEqual(30, eventDO.EndDate.Minute);
            }
        }

        [Test]
        public void TestNegativeTimezonesCorrect()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var br = fixture.TestBookingRequest1();
                var cal = fixture.TestCalendar1();
                br.Calendars.Add(cal);
                cal.BookingRequests.Add(br);
                uow.CalendarRepository.Add(cal);

                const string createDateStr = @"Sat, 29 Nov 2014 07:00:00 -0300";
                br.CreateDate = DateTimeOffset.Parse(createDateStr);
                uow.BookingRequestRepository.Add(br);

                uow.SaveChanges();

                var ec = new EventController();
                ViewResult result = ec.New(br.Id, cal.Id, "2014-11-29T11:00:00z", "2014-11-29T11:30:00z") as ViewResult;
                var model = result.Model as EventVM;

                var createdEventID = model.Id;
                var eventDO = uow.EventRepository.GetByKey(createdEventID);

                Assert.AreEqual(new TimeSpan(-3, 0, 0), eventDO.StartDate.Offset);
                Assert.AreEqual(11, eventDO.StartDate.Hour);
                Assert.AreEqual(0, eventDO.StartDate.Minute);
                Assert.AreEqual(new TimeSpan(-3, 0, 0), eventDO.EndDate.Offset);
                Assert.AreEqual(11, eventDO.EndDate.Hour);
                Assert.AreEqual(30, eventDO.EndDate.Minute);
            }
        }
    }
}
