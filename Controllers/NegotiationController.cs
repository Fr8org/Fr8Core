using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.Kwasant;
using KwasantCore.Services;
using KwasantWeb.ViewModels;
using StructureMap;
using Utilities;

namespace KwasantWeb.Controllers
{
    public class NegotiationController : Controller
    {
        Booker _booker;
        string _currBooker;
        private readonly IAttendee _attendee;
        private readonly IEmailAddress _emailAddress;
        private readonly INegotiation _negotiation;
        private readonly IAnswer _answer;
        private readonly IQuestion _question;
        private readonly BookingRequest _br;

        public NegotiationController()
        {
            _booker = new Booker();
            _attendee = ObjectFactory.GetInstance<IAttendee>();
            _emailAddress = ObjectFactory.GetInstance<IEmailAddress>();
            _negotiation = ObjectFactory.GetInstance<INegotiation>();
            _question = ObjectFactory.GetInstance<IQuestion>();
            _answer = ObjectFactory.GetInstance<IAnswer>();
            _br = new BookingRequest();
        }

        [HttpPost]
        public ActionResult CheckBooker(int bookingRequestID)
        {
            return Json(_br.VerifyCheckOut(bookingRequestID, this.GetUserId()));
        }

        public ActionResult Edit(int negotiationID, int bookingRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //First - we order by start date
                Func<NegotiationAnswerVM, DateTimeOffset?> firstSort = a => a.EventStartDate;
                //Second - order by end date
                Func<NegotiationAnswerVM, DateTimeOffset?> secondSort = a => a.EventEndDate;

                var negotiationVM = GetNegotiationVM(negotiationID, firstSort, secondSort);
                return View(negotiationVM);
            }
        }

        public ActionResult Review(int negotiationID)
        {
            //The following are order delegates.
            //We always order in ascending order, so it's important to keep that in mind.

            //First - If a question is selected, it should be at the top
            Func<NegotiationAnswerVM, long> firstSort = a => a.AnswerState == AnswerState.Selected ? 0 : 1;

            //Second - We then order a question by the number of votes. Note that because we're ordering by ascending, the more votes it has, the lower the rank will be
            //So, we subtract the votes by 1, which orders in the correct direction
            Func<NegotiationAnswerVM, long> secondSort = a => 1 - a.VotedByList.Count;

            //Third - we order events by their starting date
            Func<NegotiationAnswerVM, long> thirdSort = a => (!a.EventStartDate.HasValue ? 0 : a.EventStartDate.Value.Ticks);

            var negotiationVM = GetNegotiationVM(negotiationID, firstSort, secondSort, thirdSort);
            return View(negotiationVM);
        }

        private static NegotiationVM GetNegotiationVM<T>(int negotiationID, params Func<NegotiationAnswerVM, T>[] orders)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetQuery().FirstOrDefault(n => n.Id == negotiationID);
                if (negotiationDO == null)
                    throw new ApplicationException("Negotiation with ID " + negotiationID + " does not exist.");

                var curVM = new NegotiationVM
                {
                    Id = negotiationDO.Id,
                    Name = negotiationDO.Name,
                    BookingRequestID = negotiationDO.BookingRequestID,
                    Questions = negotiationDO.Questions.Select(q =>
                    {
                        var answers = q.Answers.Select(a =>
                            new NegotiationAnswerVM
                            {
                                Id = a.Id,
                                Text = a.Text,
                                AnswerState = a.AnswerStatus,
                                VotedByList = uow.QuestionResponseRepository.GetQuery()
                                    .Where(qr => qr.AnswerID == a.Id)
                                    .Select(qr => qr.User.UserName)
                                    .ToList(),
                                SuggestedBy = a.UserDO == null ? String.Empty : a.UserDO.UserName,
                                EventID = a.EventID,
                                EventStartDate = a.Event == null ? (DateTimeOffset?)null : a.Event.StartDate,
                                EventEndDate = a.Event == null ? (DateTimeOffset?)null : a.Event.EndDate,
                            });

                        answers = orders.Aggregate(answers, (current, t) => current.OrderBy(t));

                        return new NegotiationQuestionVM
                        {
                            AnswerType = q.AnswerType,
                            Id = q.Id,
                            CalendarID = q.CalendarID,
                            Text = q.Text,
                            Answers = answers.ToList()
                        };
                    }).ToList()
                };
                return curVM;
            }
        }


        public ActionResult Create(int bookingRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestID);

                return View("~/Views/Negotiation/Edit.cshtml",
                            new NegotiationVM
                            {
                                Name = bookingRequestDO.Subject,
                                BookingRequestID = bookingRequestID,
                                Questions = new List<NegotiationQuestionVM>
                                        {
                                            new NegotiationQuestionVM
                                                {
                                                    AnswerType = "Text"
                                                }
                                        }
                            });
            }
        }

        [HttpPost]
        public ActionResult ProcessSubmittedForm(NegotiationVM curNegotiationVM)
        {
            KwasantPackagedMessage verifyCheckoutMessage = _br.VerifyCheckOut(curNegotiationVM.BookingRequestID.Value, this.GetUserId());
            if (verifyCheckoutMessage.Name == "valid")
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    string _currBookerName = "";
                    try
                    {
                        _currBookerName = _booker.GetName(uow,
                            uow.BookingRequestRepository.GetByKey(curNegotiationVM.BookingRequestID).BookerID);

                        bool isNew = !curNegotiationVM.Id.HasValue;
                        var submittedNegotiation = AutoMapper.Mapper.Map<NegotiationVM, NegotiationDO>(curNegotiationVM);
                        var updatedNegotiationDO = _negotiation.Update(uow, submittedNegotiation);
                        uow.SaveChanges();

                        return Json(new {Name = "Success", negotiationID = updatedNegotiationDO.Id, isNew = isNew});
                    }
                    catch (Exception ex)
                    {
                        return Json(new KwasantPackagedMessage
                        {
                            Name = "Error",
                            Message =
                                " Time: " + DateTime.UtcNow + " BRId:" + curNegotiationVM.BookingRequestID +
                                " Current BR Owner Name: " + _currBookerName + " Current BR STatus: " +
                                uow.BookingRequestRepository.GetByKey(curNegotiationVM.BookingRequestID).State
                        });
                    }
                }
            }
            return Json(verifyCheckoutMessage);
        }

        public ActionResult DisplaySendEmailForm(int negotiationID, bool isNew)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationID);

                var emailController = new EmailController();

                var br = new BookingRequest();
                var emailAddresses = br.ExtractEmailAddresses(negotiationDO.BookingRequest);

                var currCreateEmailVM = new CreateEmailVM
                {
                    ToAddresses = negotiationDO.Attendees.Select(a => a.EmailAddress.Address).Where(a => !FilterUtility.IsReservedEmailAddress(a)).ToList(),
                    AddressBook = emailAddresses.ToList(),
                    Subject = string.Format("Need Your Response on {0}'s event: {1}", negotiationDO.BookingRequest.Customer.DisplayName, "RE: " + negotiationDO.Name),
                    HeaderText = String.Format("Your negotiation has been {0}. Would you like to send the emails now?", isNew
                            ? "created"
                            : "updated"),

                    BodyPromptText = "Enter some additional text for your recipients",
                    Body = "",
                    BodyRequired = false,
                    BookingRequestId = negotiationDO.BookingRequestID.Value
                };
                return emailController.DisplayEmail(Session, currCreateEmailVM,
                    (subUow, emailDO) => DispatchNegotiationEmails(subUow, emailDO, negotiationID)
                    );
            }
        }

        private ActionResult DispatchNegotiationEmails(IUnitOfWork uow, EmailDO emailDO, int negotiationID)
        {
            var communicationManager = ObjectFactory.GetInstance<CommunicationManager>();
            communicationManager.DispatchNegotiationRequests(uow, emailDO, negotiationID);

            var currBookingRequest = new BookingRequest();
            currBookingRequest.AddExpectedResponseForNegotiation(uow, emailDO, negotiationID);

            uow.SaveChanges();
            return Json(negotiationID);
        }

        [HttpPost]
        public ActionResult MarkResolved(int negotiationID)
        {
            try
            {
                //throws exception if it fails
                _negotiation.Resolve(negotiationID);
                return Json(true);
            }
            catch (EntityNotFoundException ex)
            {
                return HttpNotFound(ex.Message);
            }
        }
    }
}