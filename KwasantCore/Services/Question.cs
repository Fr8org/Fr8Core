using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using StructureMap;

namespace KwasantCore.Services
{
    public class Question : IQuestion
    {
        private readonly IAnswer _answer;

        public Question()
        {
            _answer = ObjectFactory.GetInstance<IAnswer>();
        }

        public QuestionDO Update(IUnitOfWork uow, QuestionDO submittedQuestionDO)
        {
            QuestionDO curQuestionDO;
            if (submittedQuestionDO.Id == 0)
            {
                curQuestionDO = new QuestionDO();
                uow.QuestionsRepository.Add(curQuestionDO);
            }
            else
                curQuestionDO = uow.QuestionsRepository.GetByKey(submittedQuestionDO.Id);

            curQuestionDO.AnswerType = submittedQuestionDO.AnswerType;
            if (curQuestionDO.QuestionStatus == 0)
                curQuestionDO.QuestionStatus = QuestionState.Unanswered;

            curQuestionDO.Text = submittedQuestionDO.Text;
            curQuestionDO.CalendarID = submittedQuestionDO.CalendarID;

            var proposedAnswerIDs = submittedQuestionDO.Answers.Select(a => a.Id);

            //Delete the existing answers which no longer exist in our proposed negotiation
            var existingAnswers = curQuestionDO.Answers.ToList();
            foreach (var existingAnswer in existingAnswers.Where(a => !proposedAnswerIDs.Contains(a.Id)))
            {
                uow.AnswerRepository.Remove(existingAnswer);
            }

            foreach (var submittedAnswer in submittedQuestionDO.Answers)
            {
                var updatedAnswerDO = _answer.Update(uow, submittedAnswer);
                updatedAnswerDO.Question = curQuestionDO;
            }
            return curQuestionDO;
        }
    }

}
