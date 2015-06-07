using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using KwasantTest.Fixtures;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using NUnit.Framework;
using StructureMap;
using Utilities;
using System.Web.Mvc;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;

namespace KwasantTest.Controllers
{
    [TestFixture]
    public class NegotiationControllerTests : BaseTest
    {
        [Test]
        public void Negotiation_SubmittedForm_CanGenerateNullException()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curNegotiationController = new NegotiationController();
                Assert.Throws<NullReferenceException>(() =>
                {
                    curNegotiationController.ProcessSubmittedForm(null); 
                });              
                
            }

        }


        [Test]
        public void Negotiation_SubmittedForm_CanAddNewNegotiation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation6();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                List<NegotiationQuestionVM> questionsListVM = new List<NegotiationQuestionVM>();
                List<NegotiationAnswerVM> answersListVM = new List<NegotiationAnswerVM>();

                 answersListVM.Add(new NegotiationAnswerVM{
                     Id = curNegotiationDO.Questions[0].Answers[0].Id,
                     Text = curNegotiationDO.Questions[0].Answers[0].Text,
                     AnswerState = curNegotiationDO.Questions[0].Answers[0].AnswerStatus
                 }
                 );
                
                questionsListVM.Add(new NegotiationQuestionVM { 
                    Id=curNegotiationDO.Questions[0].Id,
                    Text = curNegotiationDO.Questions[0].Text,
                    AnswerType = curNegotiationDO.Questions[0].AnswerType,
                    Answers=answersListVM
                });

                var curNegotiationController = new NegotiationController();
                var curNegotiationVM = new NegotiationVM
                {
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM     

                };
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                curNegotiationDO.BookingRequest.Booker = userDO;
                curNegotiationDO.BookingRequest.BookerID = userDO.Id;
                uow.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);
                JsonResult jsonResultActual = curNegotiationController.ProcessSubmittedForm(curNegotiationVM) as JsonResult;
                Assert.AreEqual(true, jsonResultActual.Data.ToString().Contains("Success"));
            }
        }

        [Test]
        public void Negotiation_SubmittedForm_CanUpateNegotiation()
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
                    AnswerState = curNegotiationDO.Questions[0].Answers[0].AnswerStatus
                });

                questionsListVM.Add(new NegotiationQuestionVM
                {
                    Id = curNegotiationDO.Questions[0].Id,
                    Text = curNegotiationDO.Questions[0].Text,
                    AnswerType = curNegotiationDO.Questions[0].AnswerType,
                    Answers = answersListVM
                });

                var curNegotiationController = new NegotiationController();
                var curNegotiationVM = new NegotiationVM
                {
                    Id=curNegotiationDO.Id,
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM

                };
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                curNegotiationDO.BookingRequest.Booker = userDO;
                curNegotiationDO.BookingRequest.BookerID = userDO.Id;
                uow.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);
                JsonResult jsonResultActual = curNegotiationController.ProcessSubmittedForm(curNegotiationVM) as JsonResult;
                Assert.AreEqual(true, jsonResultActual.Data.ToString().Contains("Success"));
            }
        }

        [Test]
        public void Negotiation_SubmittedForm_GetError()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curNegotiationDO = fixture.TestNegotiation6();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();

                List<NegotiationQuestionVM> questionsListVM = new List<NegotiationQuestionVM>();
                List<NegotiationAnswerVM> answersListVM = new List<NegotiationAnswerVM>();

                answersListVM.Add(new NegotiationAnswerVM
                {
                    Id = curNegotiationDO.Questions[0].Answers[0].Id,
                    Text = curNegotiationDO.Questions[0].Answers[0].Text,
                    AnswerState = curNegotiationDO.Questions[0].Answers[0].AnswerStatus
                }
                );

                questionsListVM.Add(new NegotiationQuestionVM
                {
                    Id = curNegotiationDO.Questions[0].Id,
                    Text = null,
                    AnswerType = curNegotiationDO.Questions[0].AnswerType,
                    Answers = answersListVM
                });

                var curNegotiationController = new NegotiationController();
                var curNegotiationVM = new NegotiationVM
                {
                    BookingRequestID = curNegotiationDO.BookingRequestID,
                    Name = curNegotiationDO.Name,
                    Questions = questionsListVM

                };
                var userDO = uow.UserRepository.GetOrCreateUser("testemail@gmail.com");
                uow.UserRepository.Add(userDO);
                curNegotiationDO.BookingRequest.Booker = userDO;
                curNegotiationDO.BookingRequest.BookerID = userDO.Id;
                uow.SaveChanges();
                ObjectFactory.GetInstance<ISecurityServices>().Login(uow, userDO);


                JsonResult jsonResultActual = curNegotiationController.ProcessSubmittedForm(curNegotiationVM) as JsonResult;
                Assert.AreEqual("Error", ((KwasantPackagedMessage)jsonResultActual.Data).Name);
            }

        }
       
    }
}
