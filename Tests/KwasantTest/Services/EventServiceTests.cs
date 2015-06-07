using Data.Entities;
using Data.Interfaces;
using FluentValidation;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture]
    public class EventServiceTests : BaseTest
    {

        [Test]
        [Category("Event")]
        public void CanGetOrignatorNameWithValidEmailAddress()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();

                var invitation = ObjectFactory.GetInstance<Invitation>();
                string originator = invitation.GetOriginatorName(curEvent);
                Assert.AreNotEqual(originator, null);
            }
        }

        //if first name and last name are null, but email address has a name, return the name
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_UsesEmailAddressName()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();
                UserDO curOriginator = curEvent.CreatedBy;
                curOriginator.FirstName = null;
                curOriginator.LastName = null;
                curOriginator.EmailAddress.Name = "John Smallberries";
                var invitation = ObjectFactory.GetInstance<Invitation>();
                string originator = invitation.GetOriginatorName(curEvent);
                Assert.AreEqual(originator, curOriginator.EmailAddress.Name);
            }
        }

        //if first name and last name are null, and email address does not have a name, return the first portion of the address
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_UsesEmailAddressAddress()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();
                UserDO curOriginator = curEvent.CreatedBy;
                curOriginator.FirstName = null;
                curOriginator.LastName = null;
                curOriginator.EmailAddress.Name = null;
                curOriginator.EmailAddress.Address = "john@smallberries.com";
                var invitation = ObjectFactory.GetInstance<Invitation>();
                string originator = invitation.GetOriginatorName(curEvent);
                Assert.AreEqual(originator, "john");
            }
        }

        //if first name and last name are null and email address has an invalid email address field, expect an exception
        [Test]
        [Category("Event")]
        public void CanGetOrignatorName_FailsIfNoNameData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                EventDO curEvent = fixture.TestEvent2();
                UserDO curOriginator = curEvent.CreatedBy;
                curOriginator.FirstName = null;
                curOriginator.LastName = null;
                curOriginator.EmailAddress.Name = null;
                curOriginator.EmailAddress.Address = null;

                var invitation = ObjectFactory.GetInstance<Invitation>();
                Assert.Throws<ValidationException>(() =>
                {
                    string originator = invitation.GetOriginatorName(curEvent);
                });
            }
        }
    }
}
