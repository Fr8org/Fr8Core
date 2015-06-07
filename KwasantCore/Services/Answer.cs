using Data.Entities;
using Data.Interfaces;
using KwasantCore.Interfaces;

namespace KwasantCore.Services
{
    public class Answer : IAnswer
    {
        public AnswerDO Update(IUnitOfWork uow, AnswerDO submittedAnswerDO)
        {
            AnswerDO answerDO;
            if (submittedAnswerDO.Id == 0)
            {
                answerDO = new AnswerDO();
                uow.AnswerRepository.Add(answerDO);
            }
            else
                answerDO = uow.AnswerRepository.GetByKey(submittedAnswerDO.Id);

            answerDO.EventID = submittedAnswerDO.EventID;
            answerDO.AnswerStatus = submittedAnswerDO.AnswerStatus;
            answerDO.Text = submittedAnswerDO.Text;
            return answerDO;
        }
    }
}
    

