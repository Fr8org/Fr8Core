using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using KwasantWeb.ViewModels;
using StructureMap;

namespace KwasantWeb.ViewModelServices
{
    public class NegotiationResponse
    {
        private INegotiation _negotiation;
        private ITracker _tracker;

        public NegotiationResponse()
        {
            _negotiation = ObjectFactory.GetInstance<INegotiation>();
            _tracker = ObjectFactory.GetInstance<ITracker>();

        }

        public void Process(NegotiationVM curNegotiationVM, string userID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserDO = uow.UserRepository.GetByKey(userID);
                var curNegotiationDO = uow.NegotiationsRepository.GetByKey(curNegotiationVM.Id);
                if (curNegotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");
                var questionAnswer = new Dictionary<QuestionDO, AnswerDO>();

                UpdateAnswerData(uow, curNegotiationVM, curUserDO,questionAnswer);


                if (curNegotiationDO.NegotiationState == NegotiationState.Resolved)
                {
                    AlertManager.PostResolutionNegotiationResponseReceived(curNegotiationDO.Id);
                }
                
                _negotiation.CreateQuasiEmailForBookingRequest(uow, curNegotiationDO, curUserDO, questionAnswer);

                uow.SaveChanges();

                _tracker.Identify(curUserDO);
                _tracker.Track(curUserDO, "RespondedToClarificationRequest", "ClickedNegResponseLink", new Dictionary<string, object> { { "BookingRequestId", curNegotiationDO.BookingRequestID } });
            }
        }

        public void UpdateAnswerData(IUnitOfWork uow, NegotiationVM curNegotiationVM,  UserDO curUserDO, Dictionary<QuestionDO, AnswerDO> questionAnswer )
        {
            //Here we add/update questions based on our proposed negotiation
            foreach (var submittedQuestion in curNegotiationVM.Questions)
            {
                var currentSelectedAnswers = ExtractSelectedAnswers(uow, submittedQuestion, curUserDO, questionAnswer);

                var previousQResponses = uow.QuestionResponseRepository.GetQuery()
                    .Where(qr =>
                        qr.Answer.QuestionID == submittedQuestion.Id &&
                        qr.UserID == curUserDO.Id).ToList();

                var currentSelectedAnswerIDs = submittedQuestion.Answers.Where(a => a.Selected).Select(a => a.Id).ToList();

                //First, remove old answers
                foreach (
                    var previousQResponse in
                        previousQResponses.Where(
                            previousQResponse =>
                                !previousQResponse.AnswerID.HasValue ||
                                !currentSelectedAnswerIDs.Contains(previousQResponse.AnswerID.Value)))
                {
                    uow.QuestionResponseRepository.Remove(previousQResponse);
                }

                var previousAnswerIds = previousQResponses.Select(a => a.AnswerID).ToList();

                //Add new answers
                foreach (var currentSelectedAnswer in
                        currentSelectedAnswers.Where(a => !previousAnswerIds.Contains(a.Id)))
                {
                    var newAnswer = new QuestionResponseDO
                    {
                        Answer = currentSelectedAnswer,
                        UserID = curUserDO.Id
                    };
                    uow.QuestionResponseRepository.Add(newAnswer);
                }
            }
        }


        public List<AnswerDO> ExtractSelectedAnswers(IUnitOfWork uow, NegotiationQuestionVM submittedQuestionData, UserDO curUserDO,
            Dictionary<QuestionDO, AnswerDO> questionAnswer)
        {
            if (submittedQuestionData.Id == 0)
                throw new HttpException(400, "Invalid parameter: Id of question cannot be 0.");

            var questionDO = uow.QuestionRepository.GetByKey(submittedQuestionData.Id);

            var currentSelectedAnswers = new List<AnswerDO>();

            //Previous answers are read-only, we only allow updating of new answers
            foreach (var submittedAnswerData in submittedQuestionData.Answers)
            {
                if (submittedAnswerData.Selected)
                {
                    AnswerDO answerDO;
                    if (submittedAnswerData.Id == 0)
                    {
                        answerDO = new AnswerDO();
                        uow.AnswerRepository.Add(answerDO);

                        answerDO.Question = questionDO;
                        if (answerDO.AnswerStatus == 0)
                            answerDO.AnswerStatus = AnswerState.Proposed;

                        answerDO.Text = submittedAnswerData.Text;
                        answerDO.EventID = submittedAnswerData.EventID;
                        answerDO.UserID = curUserDO.Id;
                    }
                    else
                    {
                        answerDO = uow.AnswerRepository.GetByKey(submittedAnswerData.Id);
                    }
                    questionAnswer[questionDO] = answerDO;
                    currentSelectedAnswers.Add(answerDO);
                }
            }
            return currentSelectedAnswers;
        }
    }
}

