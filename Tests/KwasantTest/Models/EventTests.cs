using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Models
{


    [TestFixture]
    public class EventTests
    {
        public IUserRepository userRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            userRepo = new UserRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        //this is a core integration test: get the ics message through
        [Test]
        [Category("Invitation")]
        public void Event_Dispatch_CanSendICS()
        {
            EventRepository eventRepo = new EventRepository(_uow);
            AttendeeRepository attendeesRepo = new AttendeeRepository(_uow);
            List<AttendeeDO> attendees =
                new List<AttendeeDO>
                {
                    _fixture.TestAttendee1(),
                    _fixture.TestAttendee2()
                };
            attendees.ForEach(attendeesRepo.Add);

            EventDO eventDO = new EventDO
            {
                Description = "This is my test invitation",
                Summary = @"My test invitation",
                Location = @"Some place!",
                StartDate = DateTime.Today.AddMinutes(5),
                EndDate = DateTime.Today.AddMinutes(15),
                Attendees = attendees,
                Emails = new List<EmailDO>()
            };
            eventRepo.Add(eventDO);
            Calendar.DispatchEvent(_uow, eventDO);

            //Verify success
            //use imap to load unread messages from the test customer account
            //verify that one of the messages is a proper ICS message
            //retry every 15 seconds for 1 minute

            

            //create an Email message addressed to the customer and attach the file.
            
           



            //skip for v.1: add EmailID to outbound queue



        }
    }
}
