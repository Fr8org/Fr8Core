using System;
using System.Net.Mail;
using System.Web.Mvc;
using FluentValidation;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Validations;
using Hub.Services;
using Utilities;
using Utilities.Logging;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{

    public class HomeController : Controller
    {
        private readonly EmailAddress _emailAddress;
        private readonly Email _email;

        public HomeController()
        {
            _emailAddress = ObjectFactory.GetInstance<EmailAddress>();
            _email = ObjectFactory.GetInstance<Email>();
        }



        public ActionResult DocuSign()
        {
            return View();
        }

        public ActionResult Index(string emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO dockyardAccountDO;
                if (!String.IsNullOrEmpty(emailAddress))
                {
                    var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress);
                    dockyardAccountDO = uow.UserRepository.GetOrCreateUser(emailAddressDO);

                    //Save incase we created..
                    uow.SaveChanges();
                }
                else
                {
                    var userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    dockyardAccountDO = uow.UserRepository.GetByKey(userID);
                }

                var returnVM = new HomeVM { SegmentWriteKey = new ConfigRepository().Get("SegmentWriteKey") };

                if (dockyardAccountDO != null)
                {
                    if (String.IsNullOrEmpty(dockyardAccountDO.FirstName))
                        returnVM.UserName = dockyardAccountDO.LastName;
                    else if (!String.IsNullOrEmpty(dockyardAccountDO.LastName))
                        returnVM.UserName = dockyardAccountDO.FirstName + " " + dockyardAccountDO.LastName;
                    else
                        returnVM.UserName = dockyardAccountDO.FirstName;

                    returnVM.UserID = dockyardAccountDO.Id;
                    returnVM.UserEmail = dockyardAccountDO.EmailAddress.Address;
                }

                return View(returnVM);
            }
        }


        public ActionResult Index_Docusign()
        {
         

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult fr8index()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult MultiPartWorkflows()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ConditionalLogicBranching()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult DataExtractionIntoSalesforce()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult DataExtractionIntoServices()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult DataExtractionintoSQLServer()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult IntegrationWithOtherDataServices()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult InterOrganizationWorkflows()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Validate emailAddress and meetingInfo then call Generate() parameterized method in BookingRequest controller
        [HttpPost]
        public ActionResult ProcessHomePageBookingRequest(string emailAddress, string meetingInfo)
        {


            RegexUtilities.ValidateEmailAddress(emailAddress);
            if (meetingInfo.Trim().Length < 30)
                return Json(new { Message = "Meeting information must have at least 30 characters" });

            return RedirectToAction("CreateViaHomePage", "BookingRequest", new { emailAddress = emailAddress, meetingInfo = meetingInfo });


        }



        //  EmailAddress  is valid then send mail .    
        // return "success" or  error 
        public ActionResult ProcessSubmittedEmail(string name, string emailId, string message)
        {
            string result = "";
            try
            {
                EmailAddressDO emailAddressDO = new EmailAddressDO(emailId);

                RegexUtilities.ValidateEmailAddress(emailAddressDO.Address);
                using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    _emailAddress.ConvertFromMailAddress(uow, new MailAddress(emailId, name));
                    string toRecipient = "info@fr8.co";
                    string fromAddress = emailId;

                    // EmailDO emailDO = email.GenerateBasicMessage(emailAddressDO, message);
                    string subject = "Customer query";
                    _email.Send(uow, subject, message, fromAddress, toRecipient);
                    //uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
                    uow.SaveChanges();
                }
                result = "success";
            }
            catch (ValidationException)
            {
                result = "You need to provide a valid Email Address.";
            }
            catch (System.Exception ex)
            {
                result = "Something went wrong with our effort to send this message. Sorry! Please try emailing your message directly to info@kwasant.com";
                Logger.GetLogger().Error("Error processing a home page email form submission.", ex);
            }
            return Content(result);
        }
        public ActionResult UserEmails()
        {
            return View();
        }
    }
}