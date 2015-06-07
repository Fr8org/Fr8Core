using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Interfaces;
using StructureMap;
using Utilities;

namespace KwasantCore.Services
{
    public class Negotiation : INegotiation
    {
        private readonly IQuestion _question;

        public Negotiation()
        {
            _question = ObjectFactory.GetInstance<IQuestion>();
        }

        //get all answers
        public List<Int32> GetAnswerIDs(NegotiationDO curNegotiationDO)
        {
            return curNegotiationDO.Questions.SelectMany(q => q.Answers.Select(a => a.Id)).ToList();
        }

        //get answers for a particular user
        public IList<Int32?> GetAnswerIDsByUser(NegotiationDO curNegotiationDO, UserDO curUserDO, IUnitOfWork uow)
        {
            var _attendee = new Attendee(new EmailAddress(new ConfigRepository()));
            var answerIDs = GetAnswerIDs(curNegotiationDO);
            return _attendee.GetRespondedAnswers(uow, answerIDs, curUserDO.Id);
        }

        public void CreateQuasiEmailForBookingRequest(IUnitOfWork uow, NegotiationDO curNegotiationDO, UserDO curUserDO, Dictionary<QuestionDO, AnswerDO> currentAnswers)
        {
            var isUpdateToAnswer = uow.NegotiationAnswerEmailRepository.GetQuery().Any(nae => nae.NegotiationID == curNegotiationDO.Id && nae.UserID == curUserDO.Id);

            var newLink = new NegotiationAnswerEmailDO();
            EmailDO quasiEmail = new EmailDO();
            newLink.Email = quasiEmail;
            newLink.User = curUserDO;
            newLink.Negotiation = curNegotiationDO;

            //Now we update it..
            const string bodyTextFormat = @"To Question: ""{0}"", answered ""{1}""";

            var bodyTextFull = new StringBuilder();
            if (isUpdateToAnswer)
                bodyTextFull.Append("*** User updated answers ***<br/>");

            //We grab each answer & question pair, and join with two line breaks.. IE:
            /*
             * To Question: "When should we meet", answered "7pm"
             * To Question: "Where should we meet", answered "Hard Rock Cafe"
             */

            bodyTextFull.Append(String.Join("<br/>", currentAnswers.Select(kvp => String.Format(bodyTextFormat, kvp.Key.Text, kvp.Value.Text))));

            quasiEmail.FromID = curUserDO.EmailAddressID;
            quasiEmail.DateReceived = DateTimeOffset.Now;

            var renderedText = bodyTextFull.ToString();
            quasiEmail.HTMLText = renderedText;
            quasiEmail.PlainText = renderedText.Replace("<br/>", Environment.NewLine);
            quasiEmail.Subject = "Response to Negotiation \"" + curNegotiationDO.Name + "\"";
            quasiEmail.ConversationId = curNegotiationDO.BookingRequestID;
            quasiEmail.EmailStatus = EmailState.Processed; //This email won't be sent

            AlertManager.ConversationMemberAdded(curNegotiationDO.BookingRequestID.Value);

            if (curNegotiationDO.BookingRequest.State != BookingRequestState.Booking)
            {
                var br = ObjectFactory.GetInstance<BookingRequest>();
                br.Reactivate(uow, curNegotiationDO.BookingRequest);
            }
            uow.EmailRepository.Add(quasiEmail);
            uow.NegotiationAnswerEmailRepository.Add(newLink);
        }

        public void Resolve(int curNegotiationId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(curNegotiationId);
                if (negotiationDO == null)
                    throw new EntityNotFoundException<NegotiationDO>(curNegotiationId);

                negotiationDO.NegotiationState = NegotiationState.Resolved;
                uow.SaveChanges();
            }
        }

        public NegotiationDO Update(IUnitOfWork uow, NegotiationDO submittedNegotiationDO)
        {
            NegotiationDO curNegotiationDO = uow.NegotiationsRepository.GetOrCreateByKey(submittedNegotiationDO.Id);
            curNegotiationDO.Name = submittedNegotiationDO.Name;
            curNegotiationDO.BookingRequestID = submittedNegotiationDO.BookingRequestID;

            var proposedQuestionIDs = submittedNegotiationDO.Questions.Select(q => q.Id);
            //Delete the existing questions which no longer exist in our proposed negotiation
            var existingQuestions = curNegotiationDO.Questions.ToList();
            foreach (var existingQuestion in existingQuestions.Where(q => !proposedQuestionIDs.Contains(q.Id)))
            {
                uow.QuestionsRepository.Remove(existingQuestion);               
            }           
            //Here we add/update questions based on our proposed negotiation
            foreach (var submittedQuestionDO in submittedNegotiationDO.Questions)
            {
                var updatedQuestionDO = _question.Update(uow, submittedQuestionDO);
                updatedQuestionDO.Negotiation = curNegotiationDO;
            }
            return curNegotiationDO;
        }

        //extract a string representation of the questions and answers for things like email
        public IEnumerable<string> GetSummaryText(NegotiationDO curNegotiationDO)
        {
            var summaryText = new List<string>();
            var actualHtml =
                    @"<strong style='color: #333333;'>{0}. {1}</strong><br/><span style='color: #333333;'>{2}</span><br/>";

            for (var i = 0; i < curNegotiationDO.Questions.Count; i++)
            {
                var question = curNegotiationDO.Questions[i];
                var currentQuestion = String.Format(actualHtml, i + 1, question.Text, question.Answers.Any() ? String.Join("<br/>", question.Answers.Select(a => a.Text)) : "[None proposed]");
                summaryText.Add(currentQuestion);
            }
            return summaryText;
        }

    }
}
