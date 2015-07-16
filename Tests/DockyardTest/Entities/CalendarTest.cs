using System;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture]
    public class CalendarDOTests : AbstractValidator<CalendarDO>
    {
        private IUnitOfWork _uow;
        private FixtureData _fixture;

        IEmailAddressRepository emailAddressRepo;
        IPersonRepository personRepo;
        ICalendarRepository calendarRepo;

        public CalendarDOTests()
        {
            RuleSet("Name", () =>
            {
                RuleFor(curCalendarDO => curCalendarDO.Name).NotNull();                
            });            
        }

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            emailAddressRepo = new EmailAddressRepository(_uow);
            personRepo = new PersonRepository(_uow);
            calendarRepo = new CalendarRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        public CalendarDO SetupCalendarForTests()
        {
            PersonDO curPersonDO = _fixture.TestPerson1();
            personRepo.Add(curPersonDO);
            personRepo.UnitOfWork.SaveChanges();

            CalendarDO calendarDO = new CalendarDO()
            {
                Name = "Calendar Test",
                PersonId = curPersonDO.PersonId
            };

            return calendarDO;
        }



        [Test]
        [Category("CalendarDO")]
        public void Calendar_Create_CanCreateCalendar()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Add(curOriginalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            //VERIFY
            CalendarDO curRetrievedCalendarDO = calendarRepo.GetByKey(curOriginalCalendarDO.CalendarId);
            Assert.AreEqual(curOriginalCalendarDO.CalendarId, curRetrievedCalendarDO.CalendarId);
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Create_FailsWithoutName()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();
            curOriginalCalendarDO.Name = null;

            //EXECUTE
            Assert.Throws<ValidationException>(() =>
             {
                 calendarRepo.Add(curOriginalCalendarDO);                 
             }
             );            
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Update_CanUpdateCalendar()
        {
            //SETUP      
            CalendarDO originalEvent = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Add(originalEvent);
            calendarRepo.UnitOfWork.SaveChanges();

            String strCalenderName = "Calendar Test Updated";
            CalendarDO updatedEvent = calendarRepo.GetByKey(originalEvent.CalendarId);
            
            updatedEvent.Name = strCalenderName;
            calendarRepo.UnitOfWork.SaveChanges();

            CalendarDO curUpdatedCalendarDO = calendarRepo.GetByKey(updatedEvent.CalendarId);

            //VERIFY
            Assert.AreEqual(curUpdatedCalendarDO.Name, strCalenderName);
            Assert.AreEqual(curUpdatedCalendarDO.PersonId, originalEvent.PersonId);
        }

        [Test]
        [Category("CalendarDO")]
        public void Calendar_Delete_CanDeleteCalendar()
        {
            //SETUP      
            CalendarDO curOriginalCalendarDO = SetupCalendarForTests();

            //EXECUTE
            calendarRepo.Add(curOriginalCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            CalendarDO curRetrievedCalendarDO = calendarRepo.GetByKey(curOriginalCalendarDO.CalendarId);

            calendarRepo.Remove(curRetrievedCalendarDO);
            calendarRepo.UnitOfWork.SaveChanges();

            //VERIFY
            CalendarDO curDeletedCalendarDO = calendarRepo.GetByKey(curRetrievedCalendarDO.CalendarId);
            Assert.IsNull(curDeletedCalendarDO);
        }
    }
}
