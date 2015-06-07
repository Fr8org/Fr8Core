using System.Net;
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
using KwasantWeb.ViewModels.JsonConverters;
using StructureMap;
using System.Net.Mail;
using System;
using Data.Repositories;
using Data.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using AutoMapper;
using Data.Validations;
using FluentValidation;
using Utilities.Logging;

namespace KwasantWeb.Controllers
{
    [KwasantAuthorize(Roles = "Booker")]
    public class BookingRequestController : Controller
    {
        // private DataTablesPackager _datatables;
        private BookingRequest _br;
        private int recordcount;
        Booker _booker;
        private JsonPackager _jsonPackager;
        private readonly IMappingEngine _mappingEngine;

        public BookingRequestController()
        {
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>();
            // _datatables = new DataTablesPackager();
            _br = new BookingRequest();
            _booker = new Booker();
            _jsonPackager = new JsonPackager();
        }

        // GET: /BookingRequest/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ShowUnprocessed()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // var jsonResult = Json(_datatables.Pack(_br.GetUnprocessed(uow)), JsonRequestBehavior.AllowGet);
                var unprocessedBRs = _br.GetUnprocessed(uow);
                var jsonResult = Json(_jsonPackager.Pack(unprocessedBRs));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        // GET: /BookingRequest/Details/5
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var currBooker = this.GetUserId();
            try
            {
                if (Request != null && Request.UrlReferrer != null)
                    if (Request.UrlReferrer.PathAndQuery == "/BookingRequest" || Request.UrlReferrer.PathAndQuery == "/BookingRequest/Index")
                        _br.ConsiderAutoCheckout(id.Value, currBooker);

                return RedirectToAction("Index", "Dashboard", new { id });
            }
            catch (EntityNotFoundException)
            {
                return HttpNotFound();
            }
        }

        //Removed. See https://maginot.atlassian.net/browse/KW-704
        //[HttpGet]
        //public ActionResult ProcessBookerChange(int bookingRequestId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var currBooker = this.GetUserId();
        //        string result = _booker.ChangeBooker(uow, bookingRequestId, currBooker);
        //        return Content(result);
        //    }
        //}

        [HttpPost]
        public ActionResult MarkAsProcessed(int curBRId)
        {
            //call to VerifyOwnership 
            KwasantPackagedMessage verifyCheckoutMessage = _br.VerifyCheckOut(curBRId, this.GetUserId());
            if (verifyCheckoutMessage.Name == "valid")
            {
                _br.MarkAsProcessed(curBRId);
                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" });
            }
            return Json(verifyCheckoutMessage);
        }

        [HttpPost]
        public ActionResult Invalidate(int curBRId)
        {
            //call to VerifyOwnership
            KwasantPackagedMessage verifyCheckoutMessage = _br.VerifyCheckOut(curBRId, this.GetUserId());
            if (verifyCheckoutMessage.Name == "valid")
            {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                    BookingRequestDO bookingRequestDO = uow.BookingRequestRepository.GetByKey(curBRId);
                bookingRequestDO.State = BookingRequestState.Invalid;
                uow.SaveChanges();
                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Status changed successfully" });
            }
        }
            return Json(verifyCheckoutMessage);
        }

        [HttpPost]
        public ActionResult ShowByUser(int? bookingRequestId, int draw, int start, int length)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string userId = _br.GetUserId(uow.BookingRequestRepository, bookingRequestId.Value);
                int recordcount = _br.GetBookingRequestsCount(uow.BookingRequestRepository, userId);
                var jsonResult = Json(new
                {
                    draw = draw,
                    recordsTotal = recordcount,
                    recordsFiltered = recordcount,
                    data = _jsonPackager.Pack(_br.GetAllByUserId(uow.BookingRequestRepository, start, length, userId))
                });

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public ActionResult ShowManualCreationForm()
        {
            return View();
        }

        //Get all checkout BR's owned by the logged
        [HttpPost]
        public ActionResult ShowByBooker()
        {
            var curBooker = this.GetUserId();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookerOwnedRequests = _br.GetCheckedOut(uow, curBooker);

                var jsonResult = Json(_jsonPackager.Pack(
                    bookerOwnedRequests.Select(
                        e =>
                        {
                            var text = e.PlainText ?? e.HTMLText;
                            if (String.IsNullOrEmpty(text))
                                text = String.Empty;
                            text = text.Trim();
                            if (text.Length > 400)
                                text = text.Substring(0, 400);

                            return new
                            {
                                id = e.Id,
                                subject = String.IsNullOrEmpty(e.Subject) ? "[No Subject]" : e.Subject,
                                fromAddress = e.From.Address,
                                dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                                body = String.IsNullOrEmpty(text) ? "[No Body]" : text
                            };
                        })
                    .ToList()
                    ));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public ActionResult ShowBRSOwnedByBooker()
        {
            return View("ShowMyBRs");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateViaBooker(string emailAddress, string meetingInfo, string subject)
        {
            try
            {
                RegexUtilities.ValidateEmailAddress(emailAddress);
                if (meetingInfo.Trim().Length < 30)
                    return Json(new { Message = "Meeting information must have at least 30 characters" });

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    string userId = _br.Generate(uow, emailAddress, meetingInfo, "SubmitsViaCreateManuallyBooker", subject);
                    return Json(new { Message = "A new booking requested created!", Result = "Success" });
                }
            }
            catch (ValidationException ex)
            {
                return Json(new { Message = "You need to provide a valid Email Address.", Result = "Failure" });
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error processing a home page try it out form schedule me", ex);
                return Json(new { Message = "Something went wrong. Sorry about that", Result = "Failure" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateViaHomePage(string emailAddress, string meetingInfo)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    string userId = _br.Generate(uow, emailAddress, meetingInfo, "SubmitsViaTryItOut", "I'm trying out Kwasant");
                    return Json(new
                        {
                            Message =
                                    "Thanks! We'll be emailing you a meeting request that demonstrates how convenient Kwasant can be",
                            UserID = userId
                        });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "Sorry! Something went wrong. Alpha software..." });
            }
        }

        // GET: /RelatedItems 
        [HttpPost]
        public ActionResult ShowRelatedItems(int bookingRequestId, int draw, int start, int length)
        {
            List<RelatedItemShowVM> obj = new List<RelatedItemShowVM>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(new
                {
                    draw = draw,
                    data = _jsonPackager.Pack(BuildRelatedItemsJSON(uow, bookingRequestId, start, length)),
                    recordsTotal = recordcount,
                    recordsFiltered = recordcount,

                });
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        public List<RelatedItemShowVM> BuildRelatedItemsJSON(IUnitOfWork uow, int bookingRequestId, int start, int length)
        {
            List<RelatedItemShowVM> bR_RelatedItems = _br
                .GetRelatedItems(uow, bookingRequestId)
                .Select(AutoMapper.Mapper.Map<RelatedItemShowVM>)
                .ToList();

            recordcount = bR_RelatedItems.Count;
            return bR_RelatedItems.OrderByDescending(x => x.Date).Skip(start).Take(length).ToList();
        }

        [HttpPost]
        public ActionResult ReleaseBooker(int bookingRequestId)
        {
            try
            {
                _br.ReleaseBooker(bookingRequestId);
                return Json(true);
            }
            catch (EntityNotFoundException)
            {
                return HttpNotFound();
            }
        }

        public ActionResult ShowInProcessBRS()
        {
            string curBooker = null;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<BookingRequestDO> bookingRequestDO = _br.GetCheckedOut(uow, curBooker);
                BookingRequestsVM curBookingRequestsVM = new BookingRequestsVM
                {
                    BookingRequests =
                        bookingRequestDO.Select(e => _mappingEngine.Map<BookingRequestDO, BookingRequestVM>(e)).ToList()
                };

                return View("ShowInProcessBRs", curBookingRequestsVM);
            }
        }

        public ActionResult DisplayOneOffEmailForm(int bookingRequestID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestID);

                var emailController = new EmailController();

                var br = new BookingRequest();
                var emailAddresses = br.ExtractEmailAddresses(bookingRequestDO);
                var userID = this.GetUserId();

                var currCreateEmailVM = new CreateEmailVM
                {
                    AddressBook = emailAddresses.ToList(),
                    Subject = bookingRequestDO.Subject,
                    SubjectEditable = true,

                    HeaderText = "Send an email",
                    BodyPromptText = "Enter some text for your recipients",
                    Body = "",
                    BookingRequestId = bookingRequestDO.Id
                };
                return emailController.DisplayEmail(Session, currCreateEmailVM,
                    (subUow, emailDO) =>
                    {
                        emailDO.TagEmailToBookingRequest(bookingRequestDO);

                        var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                        var sendingUser = subUow.UserRepository.GetByKey(userID);
                        subUow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, configRepository.Get("SimpleEmail_template"));

                        var currBookingRequest = new BookingRequest();
                        currBookingRequest.AddExpectedResponseForBookingRequest(subUow, emailDO, bookingRequestID);

                        var emailAddress = new EmailAddress(configRepository);
                        var fromEmailAddress = emailAddress.GetFromEmailAddress(subUow, emailDO.To.First(), sendingUser);
                        emailDO.FromName = fromEmailAddress.Name;
                        emailDO.From = fromEmailAddress;

                        subUow.SaveChanges();
                        return Json(true);
                    });
            }
        }


        // GET: /BookingRequest/
        public ActionResult ShowAllBookingRequests()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAllBookingRequests()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var jsonResult = Json(_jsonPackager.Pack(_br.GetAllBookingRequests(uow)));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        // GET: /BookingRequest/ShowAwaitingResponse
        public ActionResult ShowAwaitingResponse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAwaitingResponse()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string currBooker = this.GetUserId();
                var jsonResult = Json(_jsonPackager.Pack(_br.GetAwaitingResponse(uow, currBooker).Select(e => new
                {
                    id = e.Id,
                    subject = e.Subject,
                    fromAddress = e.From.Address,
                    dateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                    body =
                        e.HTMLText.Trim().Length > 400
                            ? e.HTMLText.Trim().Substring(0, 400)
                            : e.HTMLText.Trim()
                })
                    ));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        // GET: /BookingRequest/FindBookingRequest
        public ActionResult FindBookingRequest()
        {
            return View();
        }

        public PartialViewResult Search(int id, bool searchAllEmail, string subject, string body, string emailStatus, string bookingStatus)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<BRSearchResultVM> result = _br.Search(uow, id, searchAllEmail, subject, body, Convert.ToInt32(emailStatus), Convert.ToInt32(bookingStatus)).Select(e => new
                BRSearchResultVM
                {
                    Id = e.Id,
                    Subject = e.Subject,
                    From = e.From.Address,
                    DateReceived = e.DateReceived.ToString("M-d-yy hh:mm tt"),
                    BookingRequestStatus = FilterUtility.GetState(new BookingRequestState().GetType(), e.State.Value),
                    EmailStatus = FilterUtility.GetState(new EmailState().GetType(), e.EmailStatus.Value)
                }).ToList();
                return PartialView("SearchResult", result);
            }
        }

        public ActionResult AddNote(int bookingRequestId)
                {
            return View(new BookingRequestNoteVM() { BookingRequestId = bookingRequestId });
            }

        [HttpPost]
        public ActionResult SubmitNote(BookingRequestNoteVM noteVm)
        {
            var communticationManager = ObjectFactory.GetInstance<CommunicationManager>();
            communticationManager.ProcessSubmittedNote(noteVm.BookingRequestId, noteVm.Note);
            return Json(true);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CreateViaCustomer(string meetingInfo, string subject)
        {
            try
            {
                if (meetingInfo.Trim().Length < 30)
                return Json(new { Message = "Meeting information must have at least 30 characters" });

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var currUserDO = uow.UserRepository.GetByKey(this.GetUserId());
                    string userId = _br.Generate(uow, currUserDO.EmailAddress.Address, meetingInfo, "SubmitsViaCustomer", subject);
                    return Json(new { Message = "A new booking requested created!", Result = "Success" });
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error processing a home page try it out form schedule me", ex);
                return Json(new { Message = "Something went wrong. Sorry about that", Result = "Failure" });
            }
        }

        public ActionResult ShowMergeBRView(int bookingRequestID) 
        {
            ViewBag.BookingRequestId = bookingRequestID;
            return View("MergeRelatedBR");
        }

        [HttpPost]
        public ActionResult Merge(int originalBRId, int targetBRId)
        {
            //call to VerifyOwnership
            KwasantPackagedMessage verifyCheckoutMessage = _br.VerifyCheckOut(targetBRId, this.GetUserId());
            if (verifyCheckoutMessage.Name == "valid")
            {
                _br.Merge(originalBRId, targetBRId);
                TempData["isMerged"] = true;
                return Json(new KwasantPackagedMessage { Name = "Success", Message = "Merged successfully" });
            }
            return Json(verifyCheckoutMessage);
        }

        public ActionResult DefaultActivityPopup(int bookingRequestId)
        {
            ViewBag.BookingRequestId = bookingRequestId;
            return View();
        }
    }
}