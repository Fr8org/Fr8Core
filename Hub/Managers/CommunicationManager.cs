using System;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Services;

namespace Hub.Managers
{
    public class CommunicationManager
    {
        private readonly IConfigRepository _configRepository;
        private readonly EmailAddress _emailAddress;
        private readonly Fr8Account _dockyardAccount;
        //private readonly INegotiation _negotiation;
        //private readonly IBookingRequest _br;

        public CommunicationManager(IConfigRepository configRepository, EmailAddress emailAddress)
        {
            if (configRepository == null)
                throw new ArgumentNullException(nameof(configRepository));
            if (emailAddress == null)
                throw new ArgumentNullException(nameof(emailAddress));
            _configRepository = configRepository;

            _emailAddress = emailAddress;
            _dockyardAccount = ObjectFactory.GetInstance<Fr8Account>(); //can this be mocked? we would want an interface...
            //_negotiation = ObjectFactory.GetInstance<INegotiation>();
            //_br = ObjectFactory.GetInstance<IBookingRequest>(); 
        }

        //Register for interesting events
        public void SubscribeToAlerts()
        {
            EventManager.AlertExplicitCustomerCreated += NewExplicitCustomerWorkflow;
            EventManager.AlertCustomerCreated += NewCustomerWorkflow;
            //AlertManager.AlertBookingRequestNeedsProcessing += BookingRequestNeedsProcessing;
        }

	  //public void BookingRequestNeedsProcessing(int bookingRequestId)
	  //{
	  //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
	  //    {
	  //	  string toRecipient = _configRepository.Get("EmailAddress_BrNotify", null);
	  //	  if (string.IsNullOrEmpty(toRecipient))
	  //		throw new Exception("Atleast one recipient is required");
	  //	  var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
	  //	  if (!bookingRequestDO.Subject.ToLower().Contains("test message"))
	  //	  {
	  //		var email = ObjectFactory.GetInstance<Email>();
	  //		string message = "BookingRequest Needs Processing <br/>Subject : " + bookingRequestDO.Subject;
	  //		string subject = "BookingRequest Needs Processing";
	  //		string fromAddress = _configRepository.Get<string>("EmailAddress_GeneralInfo");
	  //		EmailDO curEmail = email.GenerateBasicMessage(uow, subject, message, fromAddress, toRecipient);
	  //		uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
	  //		uow.SaveChanges();
	  //	  }
	  //    }
	  //}

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewExplicitCustomerWorkflow(string curUserId)
        {
            GenerateWelcomeEmail(curUserId);
        }

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(Fr8AccountDO dockyardAccountDO)
        {
            ObjectFactory.GetInstance<ITracker>().Identify(dockyardAccountDO);
        }

        public void GenerateWelcomeEmail(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetByKey(curUserId);
                EmailDO curEmail = new EmailDO();
                curEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(_configRepository.Get("EmailAddress_GeneralInfo"));
                curEmail.AddEmailRecipient(EmailParticipantType.To, curUser.EmailAddress);
                curEmail.Subject = "Welcome to Kwasant";

                //uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, _configRepository.Get("welcome_to_kwasant_template"), null); //welcome to kwasant v2 template
                uow.SaveChanges();
            }
        }

        //public void DispatchNegotiationRequests(IUnitOfWork uow, EmailDO generatedEmailDO, int negotiationID)
        //{
        //    DispatchNegotiationRequests(uow, generatedEmailDO, uow.NegotiationsRepository.GetByKey(negotiationID));
        //}

        //public void DispatchNegotiationRequests(IUnitOfWork uow, EmailDO generatedEmailDO, NegotiationDO negotiationDO)
        //{
        //    var batches = generatedEmailDO.Recipients.GroupBy(r =>
        //    {
        //        var curUserDO = uow.UserRepository.GetOrCreateUser(r.EmailAddress);
        //        return GetCRTemplate(curUserDO);
        //    });

        //    foreach (var batch in batches)
        //    {
        //        DispatchBatchedNegotiationRequests(uow, batch.Key, generatedEmailDO.HTMLText, batch.ToList(), negotiationDO);
        //    }
        //}

        //public void DispatchBatchedNegotiationRequests(IUnitOfWork uow, String templateName, String htmlText, IList<RecipientDO> recipients, NegotiationDO negotiationDO)
        //{
        //    if (!recipients.Any())
        //        return;

        //    var emailDO = new EmailDO();
        //    //This means, when the customer replies, their client will include the bookingrequest id
        //    emailDO.TagEmailToBookingRequest(negotiationDO.BookingRequest);

        //    var customer = negotiationDO.BookingRequest.Customer;
        //    var mode = _user.GetMode(customer);
        //    if (mode == CommunicationMode.Direct)
        //    {
        //        var directEmailAddress = _configRepository.Get("EmailFromAddress_DirectMode");
        //        var directEmailName = _configRepository.Get("EmailFromName_DirectMode");
        //        emailDO.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(directEmailAddress);
        //        emailDO.FromName = directEmailName;
        //    }
        //    else
        //    {
        //        var delegateEmailAddress = _configRepository.Get("EmailFromAddress_DelegateMode");
        //        var delegateEmailName = _configRepository.Get("EmailFromName_DelegateMode");
        //        emailDO.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(delegateEmailAddress);
        //        emailDO.FromName = String.Format(delegateEmailName, customer.DisplayName);
        //    }

        //    emailDO.Subject = string.Format("Need Your Response on {0} {1} event: {2}",
        //        negotiationDO.BookingRequest.Customer.FirstName,
        //        (negotiationDO.BookingRequest.Customer.LastName ?? ""),
        //        "RE: " + negotiationDO.Name);

        //    var responseUrl = String.Format("NegotiationResponse/View?negotiationID={0}", negotiationDO.Id);

        //    var tokenUrls = new List<String>();
        //    foreach (var attendee in recipients)
        //    {
        //        emailDO.AddEmailRecipient(EmailParticipantType.To, attendee.EmailAddress);
        //        var curUserDO = uow.UserRepository.GetOrCreateUser(attendee.EmailAddress);
        //        var tokenURL = uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(responseUrl, curUserDO);
        //        tokenUrls.Add(tokenURL);
        //    }

        //    uow.EmailRepository.Add(emailDO);
        //    var summaryQandAText = _negotiation.GetSummaryText(negotiationDO);

        //    string currBookerAddress = negotiationDO.BookingRequest.Booker.EmailAddress.Address;

        //    var conversationThread = _br.GetConversationThread(negotiationDO.BookingRequest);

        //    // Fix an issue when coverting to UTF-8
        //    conversationThread = conversationThread.Replace((char)160, (char)32);

        //    //uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, templateName,
        //    //new Dictionary<string, object>
        //    //{
        //    //    {"RESP_URL", tokenUrls},
        //    //    {"bodytext", htmlText},
        //    //    {"questions", String.Join("<br/>", summaryQandAText)},
        //    //    {"conversationthread", conversationThread},
        //    //    {"bookername", currBookerAddress.Replace("@kwasant.com","")}
        //    //});

        //    negotiationDO.NegotiationState = NegotiationState.AwaitingClient;

        //    //Everyone who gets an email is now an attendee.
        //    var currentAttendeeIDs = negotiationDO.Attendees.Select(a => a.EmailAddressID).ToList();
        //    foreach (var recipient in recipients)
        //    {
        //        if (!currentAttendeeIDs.Contains(recipient.EmailAddressID))
        //        {
        //            var newAttendee = new AttendeeDO
        //            {
        //                EmailAddressID = recipient.EmailAddressID,
        //                Name = recipient.EmailAddress.Name,
        //                NegotiationID = negotiationDO.Id
        //            };
        //            uow.AttendeeRepository.Add(newAttendee);
        //        }
        //    }
        //}

        public string GetCRTemplate(Fr8AccountDO curDockyardAccountDO)
        {
            string templateName;
            // Max Kostyrkin: currently DockYardAccount#GetMode returns Direct if user has a booking request or has a password, otherwise Delegate.
            switch (_dockyardAccount.GetMode(curDockyardAccountDO))
            {
                case CommunicationMode.Direct:
                    templateName = _configRepository.Get("CR_template_for_creator");

                    break;
                case CommunicationMode.Delegate:
                    templateName = _configRepository.Get("CR_template_for_precustomer");

                    break;
                case CommunicationMode.Precustomer:
                    templateName = _configRepository.Get("CR_template_for_precustomer");

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return templateName;
        }
        //private bool EventHasChanged(IUnitOfWork uow, EventDO eventDO)
        //{
        //    //Stub method for now
        //    return true;
        //}

        //public void ProcessBRNotifications(IList<BookingRequestDO> bookingRequests)
        //{
        //    IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
        //    CommunicationConfigurationRepository communicationConfigurationRepo = uow.CommunicationConfigurationRepository;
        //    foreach (CommunicationConfigurationDO communicationConfig in communicationConfigurationRepo.GetAll().ToList())
        //    {
        //        if (communicationConfig.CommunicationType == CommunicationType.Sms)
        //        {
        //            SendBRSMSes(bookingRequests);
        //        }
        //        else if (communicationConfig.CommunicationType == CommunicationType.Email)
        //        {
        //            SendBREmails(communicationConfig.ToAddress, bookingRequests, uow);
        //        }
        //        else
        //        {
        //            throw new Exception(String.Format("Invalid communication type '{0}'", communicationConfig.CommunicationType));
        //        }
        //    }
        //    uow.SaveChanges();
        //}

        //private void SendBRSMSes(IEnumerable<BookingRequestDO> bookingRequests)
        //{
        //    if (bookingRequests.Any())
        //    {
        //        var twil = ObjectFactory.GetInstance<ISMSPackager>();
        //        string toNumber = CloudConfigurationManager.GetSetting("TwilioToNumber");
        //        twil.SendSMS(toNumber, "Inbound Email has been received");
        //    }
        //}

        //private void SendBREmails(String toAddress, IEnumerable<BookingRequestDO> bookingRequests, IUnitOfWork uow)
        //{
        //    EmailRepository emailRepo = uow.EmailRepository;
        //    const string message = "A new booking request has been created. From '{0}'.";
        //    foreach (BookingRequestDO bookingRequest in bookingRequests)
        //    {
        //        EmailDO outboundEmail = new EmailDO
        //        {
        //            Subject = "New booking request!",
        //            HTMLText = String.Format(message, bookingRequest.From.Address),
        //            EmailStatus = EmailState.Queued
        //        };

        //        var toEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(toAddress);
        //        outboundEmail.AddEmailRecipient(EmailParticipantType.To, toEmailAddress);
        //        outboundEmail.From = _emailAddress.GetFromEmailAddress(uow, toEmailAddress, bookingRequest.Customer);

        //        //uow.EnvelopeRepository.ConfigurePlainEmail(outboundEmail);
        //        emailRepo.Add(outboundEmail);
        //    }
        //}

        //public void ProcessSubmittedNote(int bookingRequestId, string note)
        //{
        //    var incidentReporter = ObjectFactory.GetInstance<IncidentReporter>();
        //    incidentReporter.ProcessSubmittedNote(bookingRequestId, note);
        //}
    }

    //public class RazorViewModel
    //{
    //    public String EmailBasicText { get { return ObjectFactory.GetInstance<IConfigRepository>().Get("emailBasicText"); } }
    //    public String UserID { get; set; }
    //    public bool IsAllDay { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public String Summary { get; set; }
    //    public String Description { get; set; }
    //    public String Location { get; set; }
    //    public String AuthTokenURL { get; set; }
    //    public List<RazorAttendeeViewModel> Attendees { get; set; }

    //    public RazorViewModel(EventDO ev, String userID, String authTokenURL)
    //    {
    //        IsAllDay = ev.IsAllDay;
    //        StartDate = ev.StartDate.DateTime;
    //        EndDate = ev.EndDate.DateTime;
    //        Summary = ev.Summary;
    //        Description = ev.Description;
    //        Location = ev.Location;
    //        AuthTokenURL = authTokenURL;
    //        Attendees = ev.Attendees.Select(a => new RazorAttendeeViewModel { Name = a.Name, EmailAddress = a.EmailAddress.Address }).ToList();
    //        UserID = userID;
    //    }

    //    public class RazorAttendeeViewModel
    //    {
    //        public String EmailAddress { get; set; }
    //        public String Name { get; set; }
    //    }
    //}
}