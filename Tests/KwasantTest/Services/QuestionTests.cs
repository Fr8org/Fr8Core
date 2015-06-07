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
    class QuestionTests:BaseTest
    {
         [Test]
         public void Question_Update_CanGenerateNullException()
         {
             using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
             {
                 var fixture = new FixtureData(uow);
                 var _question = new Question();
                 var curQuestionDO = fixture.TestQuestion4();
                 uow.QuestionRepository.Add(curQuestionDO);
                 uow.SaveChanges();
                 Assert.Throws<NullReferenceException>(() =>
                 {
                     _question.Update(uow, null);
                 });

             }
         }

         [Test]
         public void Question_Update_CanUpdateQuestion()
         {
             using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
             {
                 var fixture = new FixtureData(uow);
                 var _question = new Question();
                 var curQuestionDO = fixture.TestQuestion4();
                 var submittedQueData = fixture.TestQuestion5();
                 uow.QuestionRepository.Add(curQuestionDO);
                 uow.SaveChanges();
                 _question.Update(uow, submittedQueData);
                 uow.SaveChanges();
                 var retrievedQuestionDO = uow.QuestionRepository.GetByKey(submittedQueData.Id);
                 Assert.AreEqual(submittedQueData.Text, retrievedQuestionDO.Text);
                
             }
         }

         [Test]
         public void Question_Update_CanAddAnswer()
         {
             using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
             {
                 var fixture = new FixtureData(uow);
                 var _question = new Question();
                 var curQuestionDO = fixture.TestQuestion4();
                 uow.QuestionRepository.Add(curQuestionDO);
                 uow.SaveChanges();
                 var submittedQueData = fixture.TestQuestion6();
                 var retrievedQuestionDO = _question.Update(uow, submittedQueData);
                 uow.SaveChanges();
                 Assert.AreEqual(submittedQueData.Answers.Count, retrievedQuestionDO.Answers.Count);
             }
         }

         [Test]
         public void Question_Update_CanRemoveAnswer()
         {
             using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
             {
                 var fixture = new FixtureData(uow);
                 var _question = new Question();
                 var curQuestionDO = fixture.TestQuestion6();
                 uow.QuestionRepository.Add(curQuestionDO);
                 uow.SaveChanges();
                 var submittedQueData = fixture.TestQuestion4();
                 _question.Update(uow, submittedQueData);
                 uow.SaveChanges();
                 Assert.AreEqual(submittedQueData.Answers.Count, uow.AnswerRepository.GetQuery().Count());

             }
         }
    }
}
