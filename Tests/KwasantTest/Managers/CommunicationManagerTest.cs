using System;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;
using System.Linq;
namespace KwasantTest.Managers
{
    [TestFixture]
    public class CommunicationManagerTest : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        [Test]
        public void CanGenetrateBRNeedsProceessingEmail()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var testBookingRequest = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(testBookingRequest);
                uow.SaveChanges();

                var configRepository = ObjectFactory.GetInstance<IConfigRepository>();

                var communicationManager = new CommunicationManager(configRepository, new EmailAddress(configRepository));
                communicationManager.BookingRequestNeedsProcessing(testBookingRequest.Id);

                var emailDO = uow.EmailRepository.GetQuery().Where(e => e.Subject == "BookingRequest Needs Processing");
                Assert.AreEqual("BookingRequest Needs Processing <br/>Subject : Booking request subject", emailDO.First().HTMLText);
                Assert.AreEqual(1, emailDO.Count());
                Assert.AreEqual(1, uow.EnvelopeRepository.GetQuery().Where(e => e.EmailID == emailDO.First().Id).Count());
            }
        }

        [Test]
        public void FailsToGenetrateBRNeedsProceessingEmail() 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var testBookingRequest = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(testBookingRequest);
                uow.SaveChanges();

                var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                configRepository.Set("EmailAddress_BrNotify", "");

                var communicationManager = new CommunicationManager(configRepository, new EmailAddress(configRepository));

                Assert.Throws<Exception>(() =>
                {
                    communicationManager.BookingRequestNeedsProcessing(testBookingRequest.Id);
                });
            }
        }
    }
}
