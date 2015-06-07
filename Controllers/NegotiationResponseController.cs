using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Interfaces;
using Data.States;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using KwasantWeb.ViewModelServices;
using StructureMap;
using Utilities;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;

namespace KwasantWeb.Controllers
{
    public class NegotiationResponseController : Controller
    {
        private const bool EnforceUserInAttendees = true;
        private IAttendee _attendee;
        private Negotiation _negotiation;
        private NegotiationResponse _negotiationResponse;
        Booker _booker;

        public NegotiationResponseController()
        {
            _negotiationResponse = new NegotiationResponse();
            _negotiation = new Negotiation();
            _booker = new Booker();
        }


        //The main NegotiationResponse view displays Question and Answer data to an attendee
        [KwasantAuthorize]
        public ActionResult View(int negotiationID)
        {
            AuthenticateUser(negotiationID);

            var user = new User();
            var userID = this.GetUserId();


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userID);

                _attendee = new Attendee(new EmailAddress(new ConfigRepository()));

                var curNegotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (curNegotiationDO == null)
                    throw new HttpException(404, "Negotiation not found.");

                //get all of the Answers responded to by this user
                var userAnswerIDs = _negotiation.GetAnswerIDsByUser(curNegotiationDO, userDO, uow);

                var originatingUser = curNegotiationDO.BookingRequest.Customer.FirstName;
                if (!String.IsNullOrEmpty(curNegotiationDO.BookingRequest.Customer.LastName))
                    originatingUser += " " + curNegotiationDO.BookingRequest.Customer.LastName;


                var model = new NegotiationResponseVM
                {
                    Id = curNegotiationDO.Id,
                    Name = curNegotiationDO.Name,
                    BookingRequestID = curNegotiationDO.BookingRequestID,

                    CommunicationMode = user.GetMode(userDO),
                    OriginatingUser = originatingUser,

                    //Building the List of NegotiationQuestionVM's
                    //Starting with all of the Questions in the Negotiation...
                    Questions = curNegotiationDO.Questions.Select(q =>
                    {
                        //select the Answer that is in our list of the answers to which the  user has responded
                        // var selectedAnswer = q.Answers.FirstOrDefault(a => userAnswerIDs.Contains(a.Id));
                        var selectedAnswer = _attendee.GetSelectedAnswer(q, userAnswerIDs);


                        //build a list of NegotiationAnswerVMs
                        var answers = q.Answers.Select(a =>
                            (NegotiationAnswerVM)new NegotiationResponseAnswerVM
                            {
                                Id = a.Id,

                                //indicates which one, if any, the user has previously provided as a response 
                                Selected = a == selectedAnswer,
                                EventID = a.EventID,
                                UserAnswer = a.UserID == userID,
                                SuggestedBy = a.UserDO == null ? String.Empty : a.UserDO.UserName,

                                EventStartDate = a.Event == null ? (DateTimeOffset?)null : a.Event.StartDate,
                                EventEndDate = a.Event == null ? (DateTimeOffset?)null : a.Event.EndDate,

                                Text = a.Text,
                            }).OrderBy(a => a.EventStartDate).ThenBy(a => a.EventEndDate).ToList();

                        //We select the answer that the user previously selected
                        //If they don't have a previous selection, then we select the first answer by default. this encourages attendees to agree to what has been proposed by the booker.
                        //this Selected concept is presentation-specific, so it belongs here in the VM code
                        if (answers.Any() && !answers.Any(a => a.Selected))
                            answers.First().Selected = true;

                        //Pack the list of NegotiationAnswerVMs into a NegotiationQuestionVM
                        return (NegotiationQuestionVM)new NegotiationResponseQuestionVM
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            Text = q.Text,
                            CalendarID = q.CalendarID,

                            Answers = answers
                        };
                    }).ToList()
                };

                return View(model);
            }
        }

        [KwasantAuthorize(Roles = "Customer")]
        [HttpPost]
        public ActionResult ProcessResponse(NegotiationVM curNegotiationVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string _currBookerName = "";
                try
                {
                    _currBookerName = _booker.GetName(uow, uow.BookingRequestRepository.GetByKey(curNegotiationVM.BookingRequestID).BookerID);

                    if (!curNegotiationVM.Id.HasValue)
                        throw new HttpException(400, "Invalid parameter");

                    AuthenticateUser(curNegotiationVM.Id.Value);

                    var userID = this.GetUserId();
                    _negotiationResponse.Process(curNegotiationVM, userID);

                    return Json(new
                    {
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    return Json(new 
                    {
                        Success = false,
                        Message = " Time: " + DateTime.UtcNow + " BRId:" + curNegotiationVM.BookingRequestID + " Current BR Owner Name: " + _currBookerName + " Current BR STatus: " + uow.BookingRequestRepository.GetByKey(curNegotiationVM.BookingRequestID).State
                    });
                }
            }
        }

        [KwasantAuthorize(Roles = Roles.Customer)]
        public ActionResult ThankYouView()
        {
            ViewBag.Message = "Thank you for clarifying that. We'll get you your meeting request shortly.";
            return View();
        }


        public void AuthenticateUser(int negotiationID)
        {
            //If this is a regular customer, verify that they're an attendee
            var userID = this.GetUserId();
            var user = new User();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (!user.VerifyMinimumRole("Booker", userID, uow))
                    ConfirmUserInAttendees(uow, negotiationID);
            }
        }


        //verify that the person trying to view this negotiation is one of the attendees.
        public void ConfirmUserInAttendees(IUnitOfWork uow, int negotiationID)
        {
            if (!EnforceUserInAttendees)
                return;

            var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);
            if (negotiationDO == null)
                throw new HttpException(404, "Negotiation not found.");

            var attendees = negotiationDO.Attendees;
            var currentUserID = this.GetUserId();

            var existingUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.Id == currentUserID);
            if (existingUserDO == null)
                throw new HttpException(404, "We don't have a User record for you. ");

            var currentUserEmail = existingUserDO.EmailAddress.Address.ToLower();

            foreach (var attendee in attendees)
                if (attendee.EmailAddress.Address.ToLower() == currentUserEmail)
                    return;

            throw new HttpException(403, "You're not authorized to view information about this Negotiation");
        }

    }
}