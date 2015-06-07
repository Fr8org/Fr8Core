using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Interfaces;

namespace KwasantCore.Services
{
    class NegotiationResponse : INegotiationResponse
    {
        public void ProcessEmailedResponse(IUnitOfWork uow, EmailDO emailDO, NegotiationDO negotiationDO, string userId)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (emailDO == null)
                throw new ArgumentNullException("emailDO");
            if (negotiationDO == null)
                throw new ArgumentNullException("negotiationDO");
            foreach (var questionDO in negotiationDO.Questions)
            {
                var answerDO = new AnswerDO
                    {
                        Question = questionDO,
                        AnswerStatus = Data.States.AnswerState.Proposed,
                        UserID = userId,
                        Text = string.Format("Answered via email ID: {0}", emailDO.Id)
                    };

                uow.AnswerRepository.Add(answerDO);
            }
        }
    }
}
