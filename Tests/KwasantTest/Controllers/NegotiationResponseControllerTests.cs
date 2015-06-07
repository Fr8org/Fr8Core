using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using Data.Entities;
using Data.Infrastructure.StructureMap;
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
    class NegotiationResponseControllerTests : BaseTest
    {
        [Test]
        public void CanReturnResponseView()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation6();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                var userDO = uow.UserRepository.GetOrCreateUser("alexlucre1@gmail.com");
                uow.UserRepository.Add(userDO);
                curNegotiationDO.BookingRequest.Booker = userDO;
                curNegotiationDO.BookingRequest.BookerID = userDO.Id;
                curNegotiationDO.Attendees = fixture.TestAttendeeList1().ToList();
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);
                var curNegotiationResponseController = new NegotiationResponseController();
                ActionResult result = curNegotiationResponseController.View(curNegotiationDO.Id);

                Assert.IsInstanceOf(typeof(ViewResult), result);
            }
        }

        [Test]
        public void CanProcessNegotiationResponse()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                List<NegotiationQuestionVM> questionsListVM = new List<NegotiationQuestionVM>();
                List<NegotiationAnswerVM> answersListVM = new List<NegotiationAnswerVM>();

                answersListVM.Add(new NegotiationAnswerVM
                {
                    Id = curNegotiationDO.Questions[0].Answers[0].Id,
                    Text = curNegotiationDO.Questions[0].Answers[0].Text,
                    AnswerState = AnswerState.Selected,
                    Selected = true
                });

                questionsListVM.Add(new NegotiationQuestionVM
                {
                    Id = curNegotiationDO.Questions[0].Id,
                    Text = curNegotiationDO.Questions[0].Text,
                    AnswerType = curNegotiationDO.Questions[0].AnswerType,
                    Answers = answersListVM
                });

                var curNegotiationResponseController = new NegotiationResponseController();
                var curNegotiationVM = new NegotiationVM
                {
                    Id = curNegotiationDO.Id,
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM
                };
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                curNegotiationDO.BookingRequest.Booker = userDO;
                curNegotiationDO.BookingRequest.BookerID = userDO.Id;
                curNegotiationDO.Attendees = new List<AttendeeDO>() { new AttendeeDO() { EmailAddress = userDO.EmailAddress } };
                uow.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);
                JsonResult result = curNegotiationResponseController.ProcessResponse(curNegotiationVM) as JsonResult;
                Assert.IsTrue(uow.QuestionResponseRepository.GetQuery().Where(e => e.AnswerID == curNegotiationDO.Questions[0].Answers[0].Id && e.UserID == userDO.Id).Any());
                Assert.AreEqual(result.Data.ToString(), "{ Success = True }");
            }
        }

        [Test]
        public void CanAuthenticateUser() 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                var curNegotiationResponseController = new NegotiationResponseController();
                var userDO1 = uow.UserRepository.GetOrCreateUser("testemail1@gmail.com");
                uow.UserRepository.Add(userDO1);
                curNegotiationDO.BookingRequest.Booker = userDO1;
                curNegotiationDO.BookingRequest.BookerID = userDO1.Id;
                curNegotiationDO.Attendees = new List<AttendeeDO>() { new AttendeeDO() { EmailAddress = userDO1.EmailAddress } };
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO1);
                curNegotiationResponseController.AuthenticateUser(curNegotiationDO.Id);

                var userDO2 = uow.UserRepository.GetOrCreateUser("testemail2@gmail.com");
                uow.UserRepository.Add(userDO2);
                uow.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO2);
                Assert.Throws<HttpException>(() =>
                    {
                        curNegotiationResponseController.AuthenticateUser(curNegotiationDO.Id);
                    });
            }
        }

        [Test]
        public void CanConfirmUserInAttendees() 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                var curNegotiationResponseController = new NegotiationResponseController();
                var userDO1 = uow.UserRepository.GetOrCreateUser("testemail1@gmail.com");
                uow.UserRepository.Add(userDO1);
                curNegotiationDO.BookingRequest.Booker = userDO1;
                curNegotiationDO.BookingRequest.BookerID = userDO1.Id;
                curNegotiationDO.Attendees = new List<AttendeeDO>() { new AttendeeDO() { EmailAddress = userDO1.EmailAddress } };
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO1);
                curNegotiationResponseController.ConfirmUserInAttendees(uow, curNegotiationDO.Id);
            }
        }

        [Test]
        public void FailsToConfirmUserInAttendees() 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                var curNegotiationResponseController = new NegotiationResponseController();
                var userDO1 = uow.UserRepository.GetOrCreateUser("testemail1@gmail.com");
                uow.UserRepository.Add(userDO1);
                curNegotiationDO.BookingRequest.Booker = userDO1;
                curNegotiationDO.BookingRequest.BookerID = userDO1.Id;
                uow.SaveChanges();

                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO1);
                Assert.Throws<HttpException>(() =>
                {
                    curNegotiationResponseController.AuthenticateUser(curNegotiationDO.Id);
                });
            }
        }

        [Test]
        public void FailsToProcessNegotiationResponse()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                List<NegotiationQuestionVM> questionsListVM = new List<NegotiationQuestionVM>();
                List<NegotiationAnswerVM> answersListVM = new List<NegotiationAnswerVM>();

                answersListVM.Add(new NegotiationAnswerVM
                {
                    Id = curNegotiationDO.Questions[0].Answers[0].Id,
                    Text = curNegotiationDO.Questions[0].Answers[0].Text,
                    AnswerState = AnswerState.Selected,
                    Selected = true
                });

                questionsListVM.Add(new NegotiationQuestionVM
                {
                    Id = curNegotiationDO.Questions[0].Id,
                    Text = curNegotiationDO.Questions[0].Text,
                    AnswerType = curNegotiationDO.Questions[0].AnswerType,
                    Answers = answersListVM
                });

                var curNegotiationResponseController = new NegotiationResponseController();
                var curNegotiationVM = new NegotiationVM
                {
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM
                };
                JsonResult result = curNegotiationResponseController.ProcessResponse(curNegotiationVM) as JsonResult;
                Assert.IsTrue(result.Data.ToString().Contains("Success = False"));
            }
        }

        [Test]
        public void FailsToReturnResponseView() 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation6();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                var curNegotiationResponseController = new NegotiationResponseController();

                Assert.Throws<HttpException>(() =>
                {
                    curNegotiationResponseController.View(3);
                });
            }
        }
    }
}
