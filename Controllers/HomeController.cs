using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using FluentValidation;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Interfaces;
using Hub.Services;
using HubWeb.App_Start;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{

    public class HomeController : Controller
    {
        private readonly EmailAddress _emailAddress;
        private readonly Email _email;
        private readonly IConfigRepository _configRepository;
        public HomeController()
        {
            _emailAddress = ObjectFactory.GetInstance<EmailAddress>();
            _email = ObjectFactory.GetInstance<Email>();
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
        }
        public ActionResult Index(string emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fr8Account = ObjectFactory.GetInstance<IFr8Account>();
                if (!fr8Account.CheckForExistingAdminUsers())
                {
                    return RedirectToAction("SetupWizard", "Account");
                }

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

                var returnVM = new HomeVM { SegmentWriteKey = Fr8.Infrastructure.Utilities.Configuration.CloudConfigurationManager.GetSetting("SegmentWriteKey") };

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

        public ActionResult UnauthorizedAccess()
        {
            return View("~/shared/401.cshtml");
        }

        //  EmailAddress  is valid then send mail .    
        // return "success" or  error 
        public async Task<ActionResult> ProcessSubmittedEmail(string name, string emailId, string message)
        {
            string result = "";

            try
            {
                EmailAddressDO emailAddressDO = new EmailAddressDO(emailId);

                RegexUtilities.ValidateEmailAddress(_configRepository, emailAddressDO.Address);
                using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    _emailAddress.ConvertFromMailAddress(uow, new MailAddress(emailId, name));
                    string toRecipient = _configRepository.Get("CustomerSupportEmail");
                    string fromAddress = emailId;

                    string subject = "Customer query";
                    await _email.SendAsync(uow, subject, message, fromAddress, toRecipient);
                    uow.SaveChanges();
                }
                result = "success";
            }
            catch (ValidationException ex)
            {
                result = "You need to provide a valid Email Address.";
                Logger.GetLogger().Warn("Invalid email provided: " + emailId);
            }
            catch (Exception ex)
            {
                result = "Something went wrong with our effort to send this message. Sorry! Please try emailing your message directly to support@fr8.co";
                Logger.GetLogger().Error($"Error processing a home page email form submission. Name = {name}; EmailId = {emailId}; Exception = {ex}");
            }
            return Content(result);
        }
        public ActionResult UserEmails()
        {
            return View();
        }
    }
}