using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture]
    public class EventTests : BaseTest
    {
        
        //this is a core integration test: get the ics message through
        [Test]
        [Category("Invitation")]
        public void Event_Dispatch_CanSendICS()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var ev = ObjectFactory.GetInstance<Event>();
                var invitation = ObjectFactory.GetInstance<Invitation>();
                EventRepository eventRepo = uow.EventRepository;
            
                EventDO eventDO = fixture.TestEvent4();
                eventDO.BookingRequest = fixture.TestBookingRequest1();
               
                eventDO.BookingRequest.Booker = fixture.TestUser2();
           
                eventRepo.Add(eventDO);

                uow.SaveChanges();

                ev.GenerateInvitations(uow, eventDO, eventDO.Attendees);
                uow.SaveChanges();

                string endtime = eventDO.EndDate.ToOffset(eventDO.BookingRequest.Customer.GetOrGuessTimeZone().GetUtcOffset(DateTime.Now)).ToString("hh:mm tt");
                string subjectDate = eventDO.StartDate.ToOffset(eventDO.BookingRequest.Customer.GetOrGuessTimeZone().GetUtcOffset(DateTime.Now)).ToString("ddd MMM dd, yyyy hh:mm tt - ") + endtime;

                //Verify emails created in memory
                EmailDO resultEmail = eventDO.Emails[0];
                string expectedSubject = string.Format("Invitation from " + invitation.GetOriginatorName(eventDO) + " -- " + eventDO.Summary + " - " + subjectDate);
                Assert.AreEqual(expectedSubject, resultEmail.Subject);

                //Verify emails stored to disk properly
                EmailDO retrievedEmail = uow.EmailRepository.GetQuery().First();
                Assert.AreEqual(expectedSubject, retrievedEmail.Subject);
            }
        }

        [Test]
        [Category("Event")]
        public void Event_Add_CanAddEventWithRequiredFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP      
                EventDO curOriginalEventDO = fixture.TestEvent2();

                //EXECUTE
                uow.EventRepository.Add(curOriginalEventDO);
                uow.SaveChanges();

                //VERIFY
                EventDO curRetrievedEventDO = uow.EventRepository.GetByKey(curOriginalEventDO.Id);
                Assert.AreEqual(curOriginalEventDO.Id, curRetrievedEventDO.Id);
            }
        }

        [Test]
        [Category("Event")]
        public void Event_Add_CanAddEventWithAllFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP      
                EventDO curOriginalEventDO = fixture.TestEvent2();

                //EXECUTE
                uow.EventRepository.Add(curOriginalEventDO);
                uow.SaveChanges();

                //VERIFY
                EventDO CurRetrievedEventDO = uow.EventRepository.GetByKey(curOriginalEventDO.Id);
                Assert.AreEqual(curOriginalEventDO.Id, CurRetrievedEventDO.Id);
            }
        }

        [Test]
        [Category("Event")]
        public void Event_Add_FailAddEvenIfStartDateIsGreaterThanEndDate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP      
                EventDO curOriginalEventDO = fixture.TestEvent2();
                curOriginalEventDO.EndDate = curOriginalEventDO.StartDate.AddHours(-1);

                //EXECUTE
                Assert.Throws<ValidationException>(() =>
                {
                    uow.EventRepository.Add(curOriginalEventDO);
                }
                    );
            }
        }

        [Test]
        [Category("Event")]
        public void Event_Delete_CanDeleteEvent()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP      
                EventDO curOriginalEventDO = fixture.TestEvent1();
                curOriginalEventDO.CreatedBy = fixture.TestUser1();
                curOriginalEventDO.Attendees = new List<AttendeeDO> {fixture.TestAttendee1()};

                //EXECUTE
                uow.EventRepository.Add(curOriginalEventDO);
                uow.SaveChanges();

                EventDO curRetrievedEventDO = uow.EventRepository.GetByKey(curOriginalEventDO.Id);

                uow.EventRepository.Remove(curRetrievedEventDO);
                uow.SaveChanges();

                EventDO curDeletedEventDO = uow.EventRepository.GetByKey(curRetrievedEventDO.Id);

                //VERIFY            
                Assert.IsNull(curDeletedEventDO);
            }
        }


        //CreatesOutboundEmails when an event is confirmed
        //setup:
        //create a sample event
        //dispatch it using event#dispatch
        //verify that an email has been created

        [Test]
        [Category("Event")]
        public void Event_CreateFailsIfNoCreatedBy()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                //SETUP      
                EventDO curOriginalEventDO = fixture.TestEvent1();
                curOriginalEventDO.Attendees = new List<AttendeeDO> {fixture.TestAttendee1()};
                curOriginalEventDO.CreatedByID = null;

                //EXECUTE
                Assert.Throws<Exception>(() =>
                {
                    uow.EventRepository.Add(curOriginalEventDO);
                    uow.SaveChanges();

                }, "Property 'CreatedByID' on 'EventDO' is marked as required, but is being saved with a null value.");
            }
        }
    }
}
