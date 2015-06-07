using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using FluentValidation;
using KwasantCore.Interfaces;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;
using System.Linq;
using Data.Repositories;
using System;


namespace KwasantTest.Services
{
    [TestFixture]
    class NegotiationTests : BaseTest
    {

        
        [Test]
        public void Negotiation_Update_CanUpdateNegotiation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var _negotiation = new Negotiation();
                var curNegotiationDO = fixture.TestNegotiation1();
                var submittedNegData = fixture.TestNegotiation2();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();
                _negotiation.Update(uow, submittedNegData);
                uow.SaveChanges();
                var retrievedNegotiationDO = uow.NegotiationsRepository.GetByKey(submittedNegData.Id);
                Assert.AreEqual(submittedNegData.Name, retrievedNegotiationDO.Name);
                Assert.AreEqual(submittedNegData.BookingRequestID, retrievedNegotiationDO.BookingRequestID);

            }
        }

        [Test]
        public void Negotiation_Update_ErrorNullInputHandledProperly()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var _negotiation = new Negotiation();
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();
                Assert.Throws<NullReferenceException>(() =>
                {
                    _negotiation.Update(uow, null);
                });

            }

        }

        [Test]
        public void Negotiation_Update_CanAddQuestion()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var _negotiation = new Negotiation();
                var curNegotiationDO = fixture.TestNegotiation1();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();
                var submittedNegData = fixture.TestNegotiation3();
                var retrievedNegotiationDO = _negotiation.Update(uow, submittedNegData);
                uow.SaveChanges();
                Assert.AreEqual(submittedNegData.Questions.Count, retrievedNegotiationDO.Questions.Count);
            }
        }

        [Test]
        public void Negotiation_Update_CanRemoveQuestion()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var _negotiation = new Negotiation();
                var curNegotiationDO = fixture.TestNegotiation4();
                uow.NegotiationsRepository.Add(curNegotiationDO);
                uow.SaveChanges();
                var submittedNegData = fixture.TestNegotiation1();
                _negotiation.Update(uow, submittedNegData);
                uow.SaveChanges();
                Assert.AreEqual(submittedNegData.Questions.Count, uow.QuestionRepository.GetQuery().Count());                
               
            }
        }


       
    }
}
