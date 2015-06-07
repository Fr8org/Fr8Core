using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using Daemons;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Daemons;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;
using KwasantWeb.Controllers;

namespace KwasantTest.Services
{
    [TestFixture]
    public class BookingRequestManagerTests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            var configRepositoryMock = new Mock<IConfigRepository>();
            configRepositoryMock
                .Setup(c => c.Get<string>(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    switch (key)
                    {
                        case "MaxBRIdle":
                            return "0.04";
                        case "MaxBRReservationPeriod":
                            return "0.04";
                        case "ExpectedResponseActiveDuration":
                            return "0.04";
                        case "EmailAddress_GeneralInfo":
                            return "info@kwasant.com";
                        default:
                            return new MockedConfigRepository().Get<string>(key);
                    }
                });
            configRepositoryMock
                .Setup(c => c.Get<int>(It.IsAny<string>(), It.IsAny<int>()))
                .Returns<string, int>((key, def) =>
                {
                    switch (key)
                    {
                        case "MonitorStaleBRPeriod":
                            return 1;
                    }
                    return def;
                });

            var configRepository = configRepositoryMock.Object;
            ObjectFactory.Configure(cfg => cfg.For<IConfigRepository>().Use(configRepository));

            var notificationMock = new Mock<INotification>();
            notificationMock
                .Setup(n => n.IsInNotificationWindow(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            ObjectFactory.Configure(cfg => cfg.For<INotification>().Use(notificationMock.Object));
        }

        private void AddTestRequestData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Bookit Services")) {};

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();
            }
        }


        [Test]
        [Category("BRM")]
        public void CanProcessBRWithUnknownRequestor()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AlertReporter curAnalyticsManager = new AlertReporter();
                curAnalyticsManager.SubscribeToAlerts();

                List<UserDO> customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(0, customersNow.Count);

                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                customersNow = uow.UserRepository.GetAll().ToList();

                Assert.AreEqual(1, customersNow.Count);
                Assert.AreEqual("customer@gmail.com", customersNow.First().EmailAddress.Address);
                Assert.AreEqual("customer@gmail.com", customersNow.First().UserName);
                Assert.AreEqual("Mister Customer", customersNow.First().FirstName);
                //test analytics system

                FactDO curAction = uow.FactRepository.FindOne(k => k.ObjectId == bookingRequest.Id.ToString());
                Assert.NotNull(curAction);

                curAnalyticsManager.UnsubscribeFromAlerts();
            }
        }

        [Test]
        [Category("BRM")]
        public void CanProcessBRWithKnownRequestor()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                List<UserDO> customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(0, customersNow.Count);

                UserDO user = fixture.TestUser1();
                uow.UserRepository.Add(user);
                uow.SaveChanges();

                customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(1, customersNow.Count);

                MailMessage message = new MailMessage(new MailAddress(user.EmailAddress.Address, user.FirstName),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                customersNow = uow.UserRepository.GetAll().ToList();
                Assert.AreEqual(1, customersNow.Count);
                Assert.AreEqual(user.EmailAddress, customersNow.First().EmailAddress);
                Assert.AreEqual(user.FirstName, customersNow.First().FirstName);
            }
        }

        [Test]
        [Category("BRM")]
        public void ParseAllDay()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "CCADE",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.EventDuration.MarkAsAllDayEvent,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse30MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc30",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add30MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse60MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc60",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add60MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse90MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc90",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add90MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void Parse120MinsInAdvance()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Booqit Services"))
                {
                    Body = "cc120",
                };

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                bookingRequest = bookingRequestRepo.GetAll().ToList().First();
                Assert.AreEqual(1, bookingRequest.Instructions.Count);
                Assert.AreEqual(InstructionConstants.TravelTime.Add120MinutesTravelTime,
                    bookingRequest.Instructions.First().Id);
            }
        }

        [Test]
        [Category("BRM")]
        public void ShowUnprocessedRequestTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                object requests = (new BookingRequest()).GetUnprocessed(uow);
                object requestNow =
                    uow.BookingRequestRepository.GetAll()
                        .Where(e => e.State == BookingRequestState.NeedsBooking)
                        .OrderByDescending(e => e.Id)
                        .Select(
                            e =>
                                new
                                {
                                    request = e,
                                    body =
                                        e.HTMLText.Trim().Length > 400
                                            ? e.HTMLText.Trim().Substring(0, 400)
                                            : e.HTMLText.Trim()
                                })
                        .ToList();

                Assert.AreEqual(requestNow, requests);
            }
        }

        [Test]
        [Category("BRM")]
        public void SetStatusTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Bookit Services")) {};

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);
                bookingRequest.State = BookingRequestState.Invalid;
                uow.SaveChanges();

                IEnumerable<BookingRequestDO> requestNow =
                    uow.BookingRequestRepository.GetAll().ToList().Where(e => e.State == BookingRequestState.Invalid);
                Assert.AreEqual(1, requestNow.Count());
            }
        }

        [Test]
        [Category("BRM")]
        public void GetBookingRequestsTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AddTestRequestData();
                List<Object> requests = (new BookingRequest()).GetAllByUserId(uow.BookingRequestRepository, 0, 10, uow.BookingRequestRepository.GetAll().FirstOrDefault().Customer.Id);
                Assert.AreEqual(1, requests.Count);
            }
        }

        //This test takes too long see. KW-340. Temporarily ignoring it.
        [Test]
        [Category("BRM")]
        public void TimeOutStaleBRTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var timeOut = TimeSpan.FromSeconds(30);
                Stopwatch staleBRDuration = new Stopwatch();

                MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"),
                    new MailAddress("kwa@sant.com", "Bookit Services")) {};
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                (new BookingRequest()).Process(uow, bookingRequest);

                bookingRequest.State = BookingRequestState.Booking;
                bookingRequest.BookerID = bookingRequest.Customer.Id;
                bookingRequest.LastUpdated = DateTimeOffset.Now;
                
                uow.SaveChanges();

                staleBRDuration.Start();

                IEnumerable<BookingRequestDO> requestNow;
                do
                {
                    var om = new FreshnessMonitor();
                    DaemonTests.RunDaemonOnce(om);
                    requestNow =
                        uow.BookingRequestRepository.GetAll()
                            .ToList()
                            .Where(e => e.State == BookingRequestState.NeedsBooking);

                } while (!requestNow.Any() || staleBRDuration.Elapsed > timeOut);
                staleBRDuration.Stop();

                uow.SaveChanges();

                requestNow = uow.BookingRequestRepository.GetAll().ToList().Where(e => e.State == BookingRequestState.NeedsBooking);
                Assert.AreEqual(1, requestNow.Count());
            }
        }

        [Test]
        [Category("BRM")]
        public void GetCheckedOutTest()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = new FixtureData(uow).TestBookingRequest1();
                bookingRequestDO.State = BookingRequestState.Booking;
                bookingRequestDO.Booker = bookingRequestDO.Customer;

                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.AspNetUserRolesRepository.AssignRoleToUser("Booker", bookingRequestDO.Customer.Id);
                uow.SaveChanges();

                ObjectFactory.GetInstance<Data.Infrastructure.StructureMap.ISecurityServices>().Login(uow, bookingRequestDO.Customer);

                BookingRequestController controller = new BookingRequestController();
                controller.Details(bookingRequestDO.Id);

                IEnumerable<BookingRequestDO> requests = (new BookingRequest()).GetCheckedOut(uow, bookingRequestDO.Customer.Id);
                Assert.AreEqual(bookingRequestDO.Id, requests.FirstOrDefault().Id);
            }
        }

        [Test]
        public void CanMergeBookingRequests() 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixtureData = new FixtureData(uow);
                var testUser = fixtureData.TestUser1();
                testUser.Id = "1";
                uow.UserRepository.Add(testUser);
                uow.SaveChanges();

                var originalBookingRequest = fixtureData.TestBookingRequest1();
                var testEvent = fixtureData.TestEvent1();
                testEvent.BookingRequestID = originalBookingRequest.Id;
                uow.EventRepository.Add(testEvent);

                originalBookingRequest.Negotiations = new List<NegotiationDO>() { fixtureData.TestNegotiation1(), fixtureData.TestNegotiation2() };
                originalBookingRequest.ConversationMembers = new List<EmailDO>() { fixtureData.TestEmail1(), fixtureData.TestEmail3() };
                uow.BookingRequestRepository.Add(originalBookingRequest);

                var targetBookingRequest = fixtureData.TestBookingRequest2();
                uow.BookingRequestRepository.Add(targetBookingRequest);
                uow.SaveChanges();

                (new BookingRequest()).Merge(originalBookingRequest.Id, targetBookingRequest.Id);

                var finalBookingRequest = uow.BookingRequestRepository.GetByKey(targetBookingRequest.Id);
                Assert.AreEqual(1, uow.EventRepository.GetQuery().Where(e => e.BookingRequestID == finalBookingRequest.Id).Count());
                Assert.AreEqual(2, uow.NegotiationsRepository.GetQuery().Where(e => e.BookingRequestID == finalBookingRequest.Id).Count());
                Assert.AreEqual(2, uow.EmailRepository.GetQuery().Where(e => e.ConversationId == finalBookingRequest.Id).Count());
                Assert.AreEqual(BookingRequestState.Invalid, originalBookingRequest.State.Value);
                Assert.AreEqual(BookingRequestState.AwaitingClient, finalBookingRequest.State.Value);
            }
        }
    }
}
