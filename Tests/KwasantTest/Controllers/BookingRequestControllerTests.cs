using System;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;
using KwasantCore.Services;
using KwasantWeb.Controllers;
using NUnit.Framework;
using StructureMap;
using System.Web.Mvc;
using System.Net.Mail;
using System.Linq;
using Utilities;
using Data.Infrastructure.StructureMap;
namespace KwasantTest.Controllers
{
    public class BookingRequestControllerTests : BaseTest
    {
        private void AddTestRequestData()
        {
            MailMessage message = new MailMessage(new MailAddress("customer@gmail.com", "Mister Customer"), new MailAddress("kwa@sant.com", "Bookit Services")) { Body = String.Empty };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);

                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();
            }
        }

        [Test]
        [Category("BRM")]
        public void ShowUnprocessedRequestTest()
        {
            JsonPackager jsonPackager = new JsonPackager();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestController controller = new BookingRequestController();
                JsonResult jsonResultActual = controller.ShowUnprocessed() as JsonResult;

                string jsonResultExpected = jsonPackager.Pack((new BookingRequest()).GetUnprocessed(uow));
                Assert.AreEqual(jsonResultExpected, jsonResultActual.Data.ToString());

                AddTestRequestData();
                JsonResult jsonResultActualProcessed = controller.ShowUnprocessed() as JsonResult;
                string jsonResultExpectedProcessed = jsonPackager.Pack((new BookingRequest()).GetUnprocessed(uow));
                Assert.AreEqual(jsonResultExpectedProcessed, jsonResultActualProcessed.Data.ToString());
            }
        }

        [Test]
        [Category("BRM")]
        public void MarkAsProcessedTest()
        {
            AddTestRequestData();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

                BookingRequestController controller = new BookingRequestController();
                int id = uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
                JsonResult jsonResultActual = controller.MarkAsProcessed(id) as JsonResult;
                Assert.AreEqual("Success", ((KwasantPackagedMessage) jsonResultActual.Data).Name);
            }
        }

        [Test]
        [Category("BRM")]
        public void InvalidateTest()
        {
            AddTestRequestData();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);

                BookingRequestController controller = new BookingRequestController();
                int id = uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
                JsonResult jsonResultActual = controller.Invalidate(id) as JsonResult;
                Assert.AreEqual("Success", ((KwasantPackagedMessage) jsonResultActual.Data).Name);
            }
        }



        [Test]
        [Category("BRM")]
        public void GetBookingRequestsTest()
        {
            AddTestRequestData();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                JsonPackager jsonPackager = new JsonPackager();
                BookingRequestController controller = new BookingRequestController();
                int id = uow.BookingRequestRepository.GetAll().FirstOrDefault().Id;
                string jsonResultExpected = (new { draw = 1, recordsTotal = 1, recordsFiltered = 1, data = jsonPackager.Pack((new BookingRequest()).GetAllByUserId(uow.BookingRequestRepository, 0, 10, uow.BookingRequestRepository.GetAll().FirstOrDefault().Customer.Id)) }).ToString();
                JsonResult jsonResultActual = controller.ShowByUser(id, 1, 0, 10) as JsonResult;
                Assert.AreEqual(jsonResultExpected, jsonResultActual.Data.ToString());
            }
        }

        
    }
}
