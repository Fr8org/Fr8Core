using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Hub.Managers.APIManagers.Transmitters.Restful;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Packagers;
using Hub.Services;
using Utilities;
using Utilities.Logging;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using Data.Constants;
using Utilities.Interfaces;

//NOTES: Do NOT put Incidents here. Put them in IncidentReporter


namespace Hub.Managers
{
    public class EventReporter
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly ITerminal _terminal;
        private readonly ISecurityServices _security;

        public EventReporter(IActivityTemplate activityTemplate, ITerminal terminal)
        {
            _activityTemplate = activityTemplate;
            _terminal = terminal;
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        //Register for interesting events
        public void SubscribeToAlerts()
        {
            EventManager.AlertTrackablePropertyUpdated += TrackablePropertyUpdated;
            EventManager.AlertEntityStateChanged += EntityStateChanged;
            //AlertManager.AlertConversationMatched += AlertManagerOnAlertConversationMatched;
            EventManager.AlertEmailReceived += EmailReceived;
            EventManager.AlertEventBooked += EventBooked;
            EventManager.AlertEmailSent += EmailSent;
            //AlertManager.AlertBookingRequestCreated += BookingRequestCreated;
            EventManager.AlertExplicitCustomerCreated += CustomerCreated;

            EventManager.AlertUserRegistration += UserRegistered;

            //AlertManager.AlertBookingRequestOwnershipChange += BookingRequestOwnershipChanged;
            //AlertManager.AlertBookingRequestReserved += BookingRequestReserved;
            //AlertManager.AlertBookingRequestReservationTimeout += BookingRequestReservationTimeOut;
            //AlertManager.AlertStaleBookingRequestsDetected += StaleBookingRequestsDetected;

            //AlertManager.AlertPostResolutionNegotiationResponseReceived += OnPostResolutionNegotiationResponseReceived;

            EventManager.AlertTokenRequestInitiated += OnAlertTokenRequestInitiated;
            EventManager.AlertTokenObtained += OnAlertTokenObtained;
            EventManager.AlertTokenRevoked += OnAlertTokenRevoked;

            EventManager.EventDocuSignNotificationReceived += LogDocuSignNotificationReceived;
            EventManager.EventContainerLaunched += LogEventProcessLaunched;
            EventManager.EventCriteriaEvaluationStarted += LogEventCriteriaEvaluationStarted;
            EventManager.EventCriteriaEvaluationFinished += LogEventCriteriaEvaluationFinished;
            EventManager.EventActionStarted += LogEventActivityStarted;
            EventManager.EventActionDispatched += LogEventActivityDispatched;
            EventManager.TerminalEventReported += LogTerminalEvent;
            EventManager.TerminalActionActivated += TerminalActivityActivated;
            EventManager.EventProcessRequestReceived += EventManagerOnEventProcessRequestReceived;
            EventManager.EventContainerCreated += LogEventContainerCreated;
            EventManager.EventContainerSent += LogEventContainerSent;
            EventManager.EventContainerReceived += LogEventContainerReceived;
            EventManager.EventContainerStateChanged += LogEventContainerStateChanged;

            EventManager.EventAuthenticationCompleted += PostToTerminalEventsEndPoint;
            EventManager.EventAuthTokenCreated += AuthTokenCreated;
            EventManager.EventAuthTokenRemoved += AuthTokenRemoved;

            EventManager.EventPlanActivated += PlanActivated;
            EventManager.EventPlanDeactivated += PlanDeactivated;
            EventManager.EventContainerExecutionCompleted += ContainerExecutionCompleted;
            EventManager.EventActivityRunRequested += ActivityRunRequested;
            EventManager.EventActivityResponseReceived += ActivityResponseReceived;
            EventManager.EventProcessingTerminatedPerActivityResponse += ProcessingTerminatedPerActivityResponse;
        }

        public void UnsubscribeFromAlerts()
        {
            EventManager.AlertTrackablePropertyUpdated -= TrackablePropertyUpdated;
            EventManager.AlertEntityStateChanged -= EntityStateChanged;
            //AlertManager.AlertConversationMatched -= AlertManagerOnAlertConversationMatched;
            EventManager.AlertEmailReceived -= EmailReceived;
            EventManager.AlertEventBooked -= EventBooked;
            EventManager.AlertEmailSent -= EmailSent;
            //AlertManager.AlertBookingRequestCreated -= BookingRequestCreated;
            EventManager.AlertExplicitCustomerCreated -= CustomerCreated;

            EventManager.AlertUserRegistration -= UserRegistered;

            //AlertManager.AlertBookingRequestOwnershipChange -= BookingRequestOwnershipChanged;
            //AlertManager.AlertBookingRequestReserved -= BookingRequestReserved;
            //AlertManager.AlertBookingRequestReservationTimeout -= BookingRequestReservationTimeOut;
            //AlertManager.AlertStaleBookingRequestsDetected -= StaleBookingRequestsDetected;

            //AlertManager.AlertPostResolutionNegotiationResponseReceived -= OnPostResolutionNegotiationResponseReceived;

            EventManager.AlertTokenRequestInitiated -= OnAlertTokenRequestInitiated;
            EventManager.AlertTokenObtained -= OnAlertTokenObtained;
            EventManager.AlertTokenRevoked -= OnAlertTokenRevoked;

            EventManager.EventDocuSignNotificationReceived -= LogDocuSignNotificationReceived;
            EventManager.EventContainerLaunched -= LogEventProcessLaunched;
            EventManager.EventCriteriaEvaluationStarted -= LogEventCriteriaEvaluationStarted;
            EventManager.EventCriteriaEvaluationFinished -= LogEventCriteriaEvaluationFinished;
            EventManager.EventActionStarted -= LogEventActivityStarted;
            EventManager.EventActionDispatched -= LogEventActivityDispatched;
            EventManager.TerminalEventReported -= LogTerminalEvent;
            EventManager.TerminalActionActivated -= TerminalActivityActivated;
            EventManager.EventContainerCreated -= LogEventContainerCreated;
            EventManager.EventContainerSent -= LogEventContainerSent;
            EventManager.EventContainerReceived -= LogEventContainerReceived;
            EventManager.EventContainerStateChanged -= LogEventContainerStateChanged;

            EventManager.EventAuthenticationCompleted -= PostToTerminalEventsEndPoint;

            EventManager.EventAuthTokenCreated -= AuthTokenCreated;
            EventManager.EventAuthTokenRemoved -= AuthTokenRemoved;

            EventManager.EventPlanActivated -= PlanActivated;
            EventManager.EventPlanDeactivated -= PlanDeactivated;
            EventManager.EventContainerExecutionCompleted -= ContainerExecutionCompleted;
            EventManager.EventActivityRunRequested -= ActivityRunRequested;
            EventManager.EventActivityResponseReceived -= ActivityResponseReceived;
            EventManager.EventProcessingTerminatedPerActivityResponse -= ProcessingTerminatedPerActivityResponse;

        }

        private void ActivityResponseReceived(ActivityDO activityDo, ActivityResponse responseType)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var template = _activityTemplate.GetByKey(activityDo.ActivityTemplateId);

                var factDO = new FactDO()
                {
                    PrimaryCategory = "Container",
                    SecondaryCategory = "Activity",
                    Activity = "Process Execution",
                    Status = responseType.ToString(),
                    ObjectId = activityDo.Id.ToString(),
                    Fr8UserId = _security.GetCurrentUser(),
                    CreatedByID = _security.GetCurrentUser(),
                    Data = string.Join(
                    Environment.NewLine,
                    "Activity Name: " + template?.Name)
                };

                uow.FactRepository.Add(factDO);
                uow.SaveChanges();
            }
        }
        /*
        private void JumpToPlanRequested(PlanDO targetPlanDO, ContainerDO containerDO)
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var factDO = new FactDO()
                    {
                        PrimaryCategory = "Container",
                        SecondaryCategory = "Plan",
                        Activity = "Plan Launch",
                        Status = "Plan Launch Initiating",
                        ObjectId = targetPlanDO.Id.ToString(),
                        Fr8UserId = _security.GetCurrentUser(),
                        CreatedByID = _security.GetCurrentUser(),
                        Data = string.Join(
                            Environment.NewLine,
                            "Plan Name: " + targetPlanDO?.Name
                        )
                    };

                    uow.FactRepository.Add(factDO);
                    uow.SaveChanges();
                }

                
                //create user notifications
                var pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();

                string pusherChannel = string.Format("fr8pusher_{0}", targetPlanDO.Fr8Account.UserName);
                pusherNotifier.Notify(pusherChannel, "fr8pusher_activity_execution_info",
                    new
                    {
                        ActivityName = activityDo.Label,
                        PlanName = containerDO.Name,
                        ContainerId = containerDO.Id.ToString(),
                    });
                    
            }
            catch (Exception exception)
            {
                EventManager.UnexpectedError(exception);
            }
        }
*/
        private void ActivityRunRequested(ActivityDO activityDo, ContainerDO containerDO)
        {
            try
            {
                Guid planId;
                DateTimeOffset planLastUpdated;
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var template = _activityTemplate.GetByKey(activityDo.ActivityTemplateId);

                    var factDO = new FactDO()
                    {
                        PrimaryCategory = "Container",
                        SecondaryCategory = "Activity",
                        Activity = "Process Execution",
                        Status = "Activity Execution Initiating",
                        ObjectId = activityDo.Id.ToString(),
                        Fr8UserId = _security.GetCurrentUser(),
                        CreatedByID = _security.GetCurrentUser(),
                        Data = string.Join(
                            Environment.NewLine,
                            "Activity Name: " + template?.Name
                        )
                    };

                    uow.FactRepository.Add(factDO);
                    var planDO = uow.PlanRepository.GetById<PlanDO>(activityDo.RootPlanNodeId);
                    uow.SaveChanges();
                    planId = planDO.Id;
                    planLastUpdated = planDO.LastUpdated;
                }

                //create user notifications
                var pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();

                string pusherChannel = string.Format("fr8pusher_{0}", activityDo.Fr8Account.UserName);
                pusherNotifier.Notify(pusherChannel, "fr8pusher_activity_execution_info",
                    new
                    {
                        ActivityName = activityDo.Label,
                        PlanName = containerDO.Name,
                        ContainerId = containerDO.Id.ToString(),
                        PlanId = planId,
                        PlanLastUpdated = planLastUpdated,
                    });
            }
            catch (Exception exception)
            {
                EventManager.UnexpectedError(exception);
            }
        }

        private void ContainerExecutionCompleted(ContainerDO containerDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var factDO = new FactDO()
                {
                    PrimaryCategory = "Container Execution",
                    SecondaryCategory = "Container",
                    Activity = "Launched",
                    ObjectId = containerDO.Id.ToString(),
                    Fr8UserId = _security.GetCurrentUser(),
                    CreatedByID = _security.GetCurrentUser(),
                    Data = string.Join(
                        Environment.NewLine,
                        "Container Id: " + containerDO.Id,
                        "Plan Id: " + containerDO.PlanId
                    ),
                };

                uow.FactRepository.Add(factDO);
                uow.SaveChanges();
            }
        }

        private FactDO CreatedPlanFact(Guid planId, string state)
        {
            var factDO = new FactDO()
            {
                PrimaryCategory = "Plan",
                SecondaryCategory = "PlanState",
                Activity = "StateChanged",
                ObjectId = planId.ToString(),
                Fr8UserId = _security.GetCurrentUser(),
                CreatedByID = _security.GetCurrentUser(),
                Data = string.Join(
                Environment.NewLine,
                    "Plan State: " + state
                )
            };

            return factDO;
        }

        private void PlanDeactivated(Guid planId)
        {
            using (var uowFact = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO planDO = null;
                using (var uowPlan = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    planDO = uowPlan.PlanRepository.GetById<PlanDO>(planId);
                }
                if (planDO != null)
                {
                    var factDO = CreatedPlanFact(planId, "Deactivated");
                    uowFact.FactRepository.Add(factDO);
                    uowFact.SaveChanges();
                }
            }
        }

        private void PlanActivated(Guid planId)
        {
            using (var uowFact = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                PlanDO planDO = null;
                using (var uowPlan = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    planDO = uowPlan.PlanRepository.GetById<PlanDO>(planId);
                }
                if (planDO != null)
                {
                    var factDO = CreatedPlanFact(planId, "Activated");
                    uowFact.FactRepository.Add(factDO);
                    uowFact.SaveChanges();
                }
            }
        }

        private void ProcessingTerminatedPerActivityResponse(ContainerDO containerDO, ActivityResponse resposneType)
        {
            using (var uowFact = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var factDO = new FactDO()
                {
                    PrimaryCategory = "Container Execution",
                    SecondaryCategory = "Container",
                    Activity = "Terminated",
                    Status = resposneType.ToString(),
                    ObjectId = containerDO.Id.ToString(),
                    CreatedByID = _security.GetCurrentUser(),
                    Fr8UserId = _security.GetCurrentUser(),
                    Data = string.Join(
                    Environment.NewLine,
                   "Container Id: " + containerDO.Name)
                };

                uowFact.FactRepository.Add(factDO);
                uowFact.SaveChanges();
            }
        }
        //private void StaleBookingRequestsDetected(BookingRequestDO[] oldBookingRequests)
        //{
        //    string toNumber = ObjectFactory.GetInstance<IConfigRepository>().Get<string>("TwilioToNumber");
        //    var tw = ObjectFactory.GetInstance<ISMSPackager>();
        //    tw.SendSMS(toNumber, oldBookingRequests.Length + " Booking requests are over-due by 30 minutes.");
        //}

        //private void BookingRequestReserved(int bookingRequestId, string bookerId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var curBookingRequest = uow.BookingRequestRepository.GetByKey(bookingRequestId);
        //        if (curBookingRequest == null)
        //            throw new EntityNotFoundException<BookingRequestDO>(bookingRequestId);
        //        var curBooker = uow.UserRepository.GetByKey(bookerId);
        //        if (curBooker == null)
        //            throw new EntityNotFoundException<UserDO>(bookerId);

        //        if (!curBooker.Available.GetValueOrDefault())
        //        {
        //            IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
        //            string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

        //            const string subject = "A booking request has been reserved for you";
        //            const string messageTemplate = "A booking request has been reserved for you ({0}). Click {1} to view the booking request.";

        //            var bookingRequestURL = String.Format("{0}/BookingRequest/Details/{1}", Server.ServerUrl, curBookingRequest.Id);
        //            var message = String.Format(messageTemplate, curBookingRequest.Subject, "<a href='" + bookingRequestURL + "'>here</a>");

        //            var toRecipient = curBooker.EmailAddress;

        //            EmailDO curEmail = new EmailDO
        //            {
        //                Subject = subject,
        //                PlainText = message,
        //                HTMLText = message,
        //                From = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress),
        //                Recipients = new List<RecipientDO>
        //                    {
        //                        new RecipientDO
        //                            {
        //                                EmailAddress = toRecipient,
        //                                EmailParticipantType = EmailParticipantType.To
        //                            }
        //                    }
        //            };

        //            // uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
        //            uow.SaveChanges();
        //        }
        //    }
        //    Logger.GetLogger().Info(string.Format("Reserved. BookingRequest ID : {0}, Booker ID: {1}", bookingRequestId, bookerId));
        //}

        //private void BookingRequestReservationTimeOut(int bookingRequestId, string bookerId)
        //{

        //    Logger.GetLogger().Info(string.Format("Reservation Timed out. BookingRequest ID : {0}, Booker ID: {1}", bookingRequestId, bookerId));
        //}


        private string FormatTerminalName(AuthorizationTokenDO authorizationToken)
        {
            var terminal = _terminal.GetByKey(authorizationToken.TerminalID);

            if (terminal != null)
            {
                return terminal.Label;
            }

            return authorizationToken.TerminalID.ToString();
        }

        private void AuthTokenCreated(AuthorizationTokenDO authToken)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var factDO = new FactDO();
                factDO.PrimaryCategory = "AuthToken";
                factDO.SecondaryCategory = "Created";
                factDO.Activity = "AuthToken Created";
                factDO.ObjectId = null;
                factDO.CreatedByID = _security.GetCurrentUser();
                factDO.Data = string.Join(
                    Environment.NewLine,
                    "AuthToken method: Created",
                    "User Id: " + authToken.UserID.ToString(),
                    "Terminal name: " + FormatTerminalName(authToken),
                    "External AccountId: " + authToken.ExternalAccountId
                );

                uow.FactRepository.Add(factDO);
                uow.SaveChanges();
            }
        }

        private void AuthTokenRemoved(AuthorizationTokenDO authToken)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    PrimaryCategory = "AuthToken",
                    SecondaryCategory = "Removed",
                    Activity = "AuthToken Removed",
                    ObjectId = null,
                    CreatedByID = _security.GetCurrentUser(),
                    Data = string.Join(
                        Environment.NewLine,
                        "AuthToken method: Removed",
                        "User Id: " + authToken.UserID.ToString(),
                        "Terminal name: " + FormatTerminalName(authToken),
                        "External AccountId: " + authToken.ExternalAccountId
                    )
                };

                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private void TrackablePropertyUpdated(string entityName, string propertyName, object id,
            object value)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    PrimaryCategory = entityName,
                    SecondaryCategory = propertyName,
                    Activity = "PropertyUpdated",
                    ObjectId = id != null ? id.ToString() : null,
                    CreatedByID = _security.GetCurrentUser(),
                    Status = value != null ? value.ToString() : null,
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private void EntityStateChanged(string entityName, object id, string stateName, string stateValue)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    PrimaryCategory = entityName,
                    SecondaryCategory = stateName,
                    Fr8UserId = _security.GetCurrentUser(),
                    Activity = "StateChanged",
                    ObjectId = id != null ? id.ToString() : null,
                    CreatedByID = _security.GetCurrentUser(),
                    Status = stateValue,
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private void EventManagerOnEventProcessRequestReceived(ContainerDO containerDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(containerDO.PlanId);

                var fact = new FactDO
                {
                    //Fr8UserId = containerDO.Fr8AccountId,
                    Fr8UserId = plan.Fr8AccountId,
                    Data = containerDO.Id.ToStr(),
                    ObjectId = containerDO.Id.ToStr(),
                    PrimaryCategory = "Process Access",
                    SecondaryCategory = "Process",
                    Activity = "Requested"
                };

                SaveAndLogFact(fact);
            }
        }


        //private void AlertManagerOnAlertConversationMatched(int emailID, string subject, int bookingRequestID)
        //{
        //    const string logMessageFormat = "Inbound Email ID {0} with subject '{1}' was matched to BR ID {2}";
        //    var logMessage = String.Format(logMessageFormat, emailID, subject, bookingRequestID);

        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var incidentDO = new IncidentDO
        //        {
        //            ObjectId = emailID.ToString(),
        //            PrimaryCategory = "BookingRequest",
        //            SecondaryCategory = "Conversation",
        //            Data = logMessage
        //        };
        //        uow.IncidentRepository.Add(incidentDO);
        //        uow.SaveChanges();
        //    }

        //    Logger.GetLogger().Info(logMessage);
        //}

        //private static void OnPostResolutionNegotiationResponseReceived(int negotiationId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var negotiationDO = uow.NegotiationsRepository.GetByKey(negotiationId);

        //        IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
        //        string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

        //        const string subject = "New response to resolved negotiation request";
        //        const string messageTemplate = "A customer has submitted a new response to an already-resolved negotiation request ({0}). Click {1} to view the booking request.";

        //        var bookingRequestURL = String.Format("{0}/BookingRequest/Details/{1}", Server.ServerUrl, negotiationDO.BookingRequestID);
        //        var message = String.Format(messageTemplate, negotiationDO.Name, "<a href='" + bookingRequestURL + "'>here</a>");

        //        var toRecipient = negotiationDO.BookingRequest.Booker.EmailAddress;

        //        EmailDO curEmail = new EmailDO
        //        {
        //            Subject = subject,
        //            PlainText = message,
        //            HTMLText = message,
        //            From = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress),
        //            Recipients = new List<RecipientDO>
        //                    {
        //                        new RecipientDO
        //                            {
        //                                EmailAddress = toRecipient,
        //                                EmailParticipantType = EmailParticipantType.To
        //                            }
        //                    }
        //        };

        //        //  uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
        //        uow.SaveChanges();
        //    }
        //}

        private void CustomerCreated(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curAction = new FactDO
                {
                    PrimaryCategory = "User",
                    SecondaryCategory = "",
                    Activity = "Created",
                    Fr8UserId = curUserId,
                    ObjectId = null,
                    Data = string.Format("User with email :{0}, created from: {1}", uow.UserRepository.GetByKey(curUserId).EmailAddress.Address, new StackTrace())
                };

                SaveFact(curAction);
            }
        }

        public void EmailReceived(int emailId, string Fr8UserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                string emailSubject = uow.EmailRepository.GetByKey(emailId).Subject;
                emailSubject = emailSubject.Length <= 10 ? emailSubject : (emailSubject.Substring(0, 10) + "...");

                FactDO curAction = new FactDO
                {
                    PrimaryCategory = "Email",
                    SecondaryCategory = "",
                    Activity = "Received",
                    Fr8UserId = Fr8UserId,
                    ObjectId = emailId.ToString(CultureInfo.InvariantCulture)
                };

                curAction.Data = string.Format("{0} ID :{1}, {2} {3}: ObjectId: {4} EmailAddress: {5} Subject: {6}", curAction.PrimaryCategory, emailId, curAction.SecondaryCategory, curAction.Activity, emailId, (uow.UserRepository.GetByKey(curAction.Fr8UserId).EmailAddress.Address), emailSubject);

                SaveFact(curAction);
            }
        }

        public void EventBooked(int eventId, string Fr8UserId)
        {
            FactDO curAction = new FactDO
            {
                PrimaryCategory = "Event",
                SecondaryCategory = "",
                Activity = "Booked",
                Fr8UserId = Fr8UserId,
                ObjectId = eventId.ToString(CultureInfo.InvariantCulture)
            };
            SaveFact(curAction);
        }
        public void EmailSent(int emailId, string Fr8UserId)
        {
            FactDO curAction = new FactDO
            {
                PrimaryCategory = "Email",
                SecondaryCategory = "",
                Activity = "Sent",
                Fr8UserId = Fr8UserId,
                ObjectId = emailId.ToString(CultureInfo.InvariantCulture)
            };
            SaveFact(curAction);
        }

        //public void BookingRequestCreated(int bookingRequestId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);


        //        ObjectFactory.GetInstance<ITracker>().Track(bookingRequestDO.Customer, "BookingRequest", "Submit", new Dictionary<string, object> { { "BookingRequestId", bookingRequestDO.Id } });

        //        FactDO curAction = new FactDO
        //        {
        //            PrimaryCategory = "BookingRequest",
        //            SecondaryCategory = "",
        //            Activity = "Created",
        //            Fr8UserId = bookingRequestDO.Fr8UserId,
        //            ObjectId = bookingRequestId.ToString(CultureInfo.InvariantCulture)
        //        };

        //        curAction.Data = string.Format("{0} ID :{1},", curAction.PrimaryCategory, curAction.ObjectId);
        //        SaveFact(curAction);
        //    }
        //}

        /// <summary>
        /// The method logs the fact of receiving a notification from DocuSign.      
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="envelopeId">EnvelopeId received from DocuSign.</param>
        public void DocusignNotificationReceived(string userId, string envelopeId)
        {
            FactDO fact = new FactDO
            {
                PrimaryCategory = "Notification",
                SecondaryCategory = null,
                Activity = "Received",
                Fr8UserId = userId,
                ObjectId = null,
                Data = string.Format("EnvelopeId: {0}.",
                        envelopeId)
            };
            LogHistoryItem(fact);
            //LogFactInformation(fact, "DocusignNotificationReceived");
            SaveFact(fact);
        }

        /// <summary>
        /// The method logs the fact of Process Template creation.      
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="planId">EnvelopeId received from DocuSign.</param>
        public void RouteCreated(string userId, string planName)
        {
            FactDO fact = new FactDO
            {
                PrimaryCategory = "PlanService",
                SecondaryCategory = null,
                Activity = "Created",
                Fr8UserId = userId,
                ObjectId = "0",
                Data = string.Format("Plan Name: {0}.",
                        planName)
            };
            LogHistoryItem(fact);
            //LogFactInformation(fact, "RouteCreated");
            SaveFact(fact);
        }

        /// <summary>
        /// The method logs the fact of receiving a notification from DocuSign.      
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="envelopeId">EnvelopeId received from DocuSign.</param>
        public void ImproperDocusignNotificationReceived(string message)
        {
            var fact = new IncidentDO
            {
                Fr8UserId = _security.GetCurrentUser(),
                PrimaryCategory = "Notification",
                Activity = "Received",
                Data = message
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(fact);
                uow.SaveChanges();
            }
            LogHistoryItem(fact,EventType.Warning);
            //LogFactInformation(fact, "ImproperDocusignNotificationReceived", EventType.Warning);
        }

        /// <summary>
        /// The method records information about unhandled exceptions. 
        /// </summary>
        /// <param name="message"></param>
        public void UnhandledErrorCaught(string message)
        {
            var incidentDO = new IncidentDO
            {
                Fr8UserId = _security.GetCurrentUser(),
                PrimaryCategory = "Error",
                SecondaryCategory = "ApplicationException",
                Activity = "Received",
                Data = message
            };

            Logger.GetLogger().Error(message);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(incidentDO);

                //The error may be connected to the fact that DB is unavailable, 
                //we need to be prepared to that. 
                try
                {
                    uow.SaveChanges();
                }
                catch { }
            }
        }

        /// <summary>
        /// The method logs the fact of processing of a notification from DocuSign
        /// by an individual Process.
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="envelopeId">EnvelopeId received from DocuSign.</param>
        public void AlertProcessProcessing(string userId, string envelopeId, int containerId)
        {
            FactDO fact = new FactDO
            {
                PrimaryCategory = "Notification",
                SecondaryCategory = null,
                Activity = "Processed",
                Fr8UserId = userId,
                ObjectId = null,
                Data = string.Format("A notification from DocuSign is processed. UserId: {0}, EnvelopeId: {1}, ContainerDO id: {2}.",
                        userId,
                        envelopeId,
                        containerId)
            };
            LogHistoryItem(fact);
            //LogFactInformation(fact, "ProcessProcessing");
            SaveFact(fact);
        }

        private void SaveFact(FactDO curAction)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.FactRepository.Add(curAction);
                uow.SaveChanges();
            }
        }

        private void SaveAndLogFact(FactDO fact)
        {
            SaveFact(fact);
            LogHistoryItem(fact);
            //LogFactInformation(fact, fact.SecondaryCategory + " " + fact.Activity);
        }

        public void UserRegistered(Fr8AccountDO curUser)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curFactDO = new FactDO
                {
                    PrimaryCategory = "User",
                    SecondaryCategory = "",
                    Activity = "Registered",
                    Fr8UserId = curUser.Id,
                    ObjectId = null,
                    Data = string.Format("User registrated with :{0},", curUser.EmailAddress.Address)
                    //Data = "User registrated with " + curUser.EmailAddress.Address
                };
                Logger.GetLogger().Info(curFactDO.Data);
                uow.FactRepository.Add(curFactDO);
                uow.SaveChanges();
            }
        }

        public void ActivityTemplatesSuccessfullyRegistered(int count)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curFactDO = new FactDO
                {
                    PrimaryCategory = "StartUp",
                    SecondaryCategory = "Activity Templates",
                    Activity = "Registered",
                    ObjectId = null,
                    Data = string.Format("{0} activity templates were registrated", count)
                    //Data = "User registrated with " + curUser.EmailAddress.Address
                };
                Logger.GetLogger().Info(curFactDO.Data);
                uow.FactRepository.Add(curFactDO);
                uow.SaveChanges();
            }
        }

        public void ActivityTemplateTerminalRegistrationError(string message, string exceptionType)
        {
            var incidentDO = new IncidentDO
            {
                PrimaryCategory = "Startup",
                SecondaryCategory = "Terminal Registration",
                Activity = "Registration Failure",
                Data = message
            };

            Logger.GetLogger().Error(message);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(incidentDO);

                //The error may be connected to the fact that DB is unavailable, 
                //we need to be prepared to that. 
                try
                {
                    uow.SaveChanges();
                }
                catch { }
            }
        }


        private void AddFactOnToken(string userId, string activity)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO factDO = new FactDO
                {
                    PrimaryCategory = "DocuSign",
                    SecondaryCategory = "Token",
                    Activity = activity,
                    Fr8UserId = userId,
                };

                uow.FactRepository.Add(factDO);
                uow.SaveChanges();
            }
        }

        public string ComposeOutputString(HistoryItemDO historyItem)
        {
            string itemType = historyItem.GetType().Name.Replace("DO", "");
            var message = $"{itemType}: {historyItem.PrimaryCategory} " +
                              $"{historyItem.SecondaryCategory}" +
                              $"{historyItem.Activity}, " +
                              $"Data = {historyItem.Data}, " +
                              $"Fr8User = {historyItem.Fr8UserId}, " +
                              $"ObjectId = {historyItem.ObjectId}";

            return message;
        }

        /// <summary>
        /// Logs historyItem information using the standard log mechanisms, replacement for LogFactInformation .
        /// </summary>
        /// <param name="fact">An instance of FactDO class.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="eventType">Event type.</param>
        public void LogHistoryItem(HistoryItemDO historyItem, EventType eventType = EventType.Info)
        {
            var message = ComposeOutputString(historyItem);

            switch (eventType)
            {
                case EventType.Info:
                    Logger.GetLogger().Info(message);
                    break;
                case EventType.Error:
                    Logger.GetLogger().Error(message);
                    break;
                case EventType.Warning:
                    Logger.GetLogger().Warn(message);
                    break;
            }
        }

        /// <summary>
        /// Logs fact information using the standard log mechanisms.
        /// </summary>
        /// <param name="fact">An instance of FactDO class.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="eventType">Event type.</param>
        //public void LogFactInformation(HistoryItemDO fact, string eventName, EventType eventType = EventType.Info)
        //{
        //    string message = string.Format(
        //        "Event {0} generated with Fr8UserId = {1}, ObjectId = {2} and Data = {3}.",
        //        eventName,
        //        fact.Fr8UserId,
        //        fact.ObjectId,
        //        fact.Data);

        //    switch (eventType)
        //    {
        //        case EventType.Info:
        //            Logger.GetLogger().Info(message);
        //            break;
        //        case EventType.Error:
        //            Logger.GetLogger().Error(message);
        //            break;
        //        case EventType.Warning:
        //            Logger.GetLogger().Warn(message);
        //            break;
        //    }
        //}

        private void OnAlertTokenRequestInitiated(string userId)
        {
            AddFactOnToken(userId, "Requested");
        }

        private void OnAlertTokenObtained(string userId)
        {
            AddFactOnToken(userId, "Obtained");
        }

        private void OnAlertTokenRevoked(string userId)
        {
            AddFactOnToken(userId, "Revoked");
        }

        private void LogDocuSignNotificationReceived()
        {
            var fact = new FactDO
            {
                Fr8UserId = null,
                Data = "DocuSign Notificaiton Received",
                ObjectId = null,
                PrimaryCategory = "External Event",
                SecondaryCategory = "DocuSign",
                Activity = "Received"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventProcessLaunched(ContainerDO launchedContainer)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(launchedContainer.PlanId);

                var fact = new FactDO
                {
                    Fr8UserId = plan.Fr8AccountId,
                    Data = launchedContainer.Id.ToStr(),
                    ObjectId = launchedContainer.Id.ToStr(),
                    PrimaryCategory = "Container Execution",
                    SecondaryCategory = "Container",
                    Activity = "Launched"
                };

                SaveAndLogFact(fact);
            }
        }

        private void LogEventCriteriaEvaluationStarted(Guid containerId)
        {
            ContainerDO containerInExecution;
            FactDO fact;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                containerInExecution = uow.ContainerRepository.GetByKey(containerId);
                var plan = containerInExecution != null ? uow.PlanRepository.GetById<PlanDO>(containerInExecution.PlanId) : null; ;

                fact = new FactDO
                {
                    Fr8UserId = containerInExecution != null ? plan.Fr8AccountId : "unknown",
                    Data = containerInExecution != null ? containerInExecution.Id.ToStr() : "unknown",
                    ObjectId = null,
                    PrimaryCategory = "Process Execution",
                    SecondaryCategory = "Criteria Evaluation",
                    Activity = "Started"
                };
            }

            SaveAndLogFact(fact);
        }

        private void LogEventCriteriaEvaluationFinished(Guid curContainerId)
        {
            ContainerDO containerInExecution;
            FactDO fact;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                containerInExecution = uow.ContainerRepository.GetByKey(curContainerId);
                var plan = containerInExecution != null ? uow.PlanRepository.GetById<PlanDO>(containerInExecution.PlanId) : null;

                fact = new FactDO
                {
                    Fr8UserId = containerInExecution != null ? plan.Fr8AccountId : "unknown",
                    Data = containerInExecution != null ? containerInExecution.Id.ToStr() : "unknown",
                    ObjectId = null,
                    PrimaryCategory = "Process Execution",
                    SecondaryCategory = "Criteria Evaluation",
                    Activity = "Finished"
                };
            }

            SaveAndLogFact(fact);
        }

        private void LogEventActivityStarted(ActivityDO curActivity, ContainerDO containerInExecution)
        {
            FactDO fact;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = containerInExecution != null ? uow.PlanRepository.GetById<PlanDO>(containerInExecution.PlanId) : null;

                fact = new FactDO
                {
                    Fr8UserId = (containerInExecution != null) ? plan.Fr8AccountId : "unknown",
                    Data = (containerInExecution != null) ? containerInExecution.Id.ToStr() : "unknown",
                    ObjectId = curActivity.Id.ToStr(),
                    PrimaryCategory = "Process Execution",
                    SecondaryCategory = "Action",
                    Activity = "Started"
                };
            }

            SaveAndLogFact(fact);
        }

        // Commented by Vladimir. DO-1214. If one action can have only one Process?
        private void LogEventActivityDispatched(ActivityDO curActivity, Guid processId)
        {
            ContainerDO containerInExecution;
            FactDO fact;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                containerInExecution = uow.ContainerRepository.GetByKey(processId);
                var plan = containerInExecution != null ? uow.PlanRepository.GetById<PlanDO>(containerInExecution.PlanId) : null;

                fact = new FactDO
                {
                    Fr8UserId = containerInExecution != null ? plan.Fr8AccountId : "unknown",
                    Data = containerInExecution != null ? containerInExecution.Id.ToStr() : "unknown",
                    ObjectId = curActivity.Id.ToStr(),
                    PrimaryCategory = "Process Execution",
                    SecondaryCategory = "Action",
                    Activity = "Dispatched"
                };
            }

            SaveAndLogFact(fact);
        }

        private void LogTerminalEvent(LoggingDataCm eventDataCm)
        {
            var fact = new FactDO
            {
                ObjectId = eventDataCm.ObjectId,
                Fr8UserId = eventDataCm.Fr8UserId,
                Data = eventDataCm.Data,
                PrimaryCategory = eventDataCm.PrimaryCategory,
                SecondaryCategory = eventDataCm.SecondaryCategory,
                Component = "Terminal",
                Activity = eventDataCm.Activity
            };

            SaveAndLogFact(fact);
        }

        // Commented by Vladimir. DO-1214. If one action can have only one Process?
        private void TerminalActivityActivated(ActivityDO curActivity)
        {
            //            ProcessDO processInExecution;
            //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //            {
            //                int? processId = uow.ActionListRepository.GetByKey(curAction.ParentActivityId).ProcessID;
            //                processInExecution = uow.ProcessRepository.GetByKey(processId);
            //            }
            //
            //            var fact = new FactDO
            //            {
            //                Fr8UserId = processInExecution.DockyardAccountId,
            //                Data = processInExecution.Id.ToStr(),
            //                ObjectId = curAction.Id.ToStr(),
            //                PrimaryCategory = "Action",
            //                SecondaryCategory = "Activation",
            //                Activity = "Completed"
            //            };
            //
            //            SaveAndLogFact(fact);
        }

        private void LogEventContainerCreated(ContainerDO containerDO)
        {
            CreateContainerFact(containerDO, "Created");
        }
        private void LogEventContainerSent(ContainerDO containerDO, ActivityDO activityDO)
        {
            CreateContainerFact(containerDO, "Sent To Terminal", activityDO);
        }
        private void LogEventContainerReceived(ContainerDO containerDO, ActivityDO activityDO)
        {
            CreateContainerFact(containerDO, "Received From Terminal", activityDO);
        }
        private void LogEventContainerStateChanged(DbPropertyValues currentValues)
        {
            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            //In the GetByKey I make use of dictionary datatype: https://msdn.microsoft.com/en-us/data/jj592677.aspx
            var curContainerDO = uow.ContainerRepository.GetByKey(currentValues[currentValues.PropertyNames.First()]);
            CreateContainerFact(curContainerDO, "StateChanged");


        }

        public enum EventType
        {
            Info,
            Error,
            Warning
        }


        private void CreateContainerFact(ContainerDO containerDO, string activity, ActivityDO activityDO = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(containerDO.PlanId);

                var curFact = new FactDO
                {
                    Fr8UserId = plan.Fr8AccountId,
                    ObjectId = containerDO.Id.ToStr(),
                    PrimaryCategory = "Containers",
                    SecondaryCategory = "Operations",
                    Activity = activity
                };
                if (activityDO != null)
                {
                    var activityTemplate = _activityTemplate.GetByKey(activityDO.ActivityTemplateId);
                    curFact.Data = string.Format("Terminal: {0} - Action: {1}.", activityTemplate.Terminal.Name, activityTemplate.Name);
                }

                LogHistoryItem(curFact);
                //LogFactInformation(curFact, curFact.Data);
                uow.FactRepository.Add(curFact);
                uow.SaveChanges();
            }
        }

        private static async Task PostToTerminalEventsEndPoint(string userId, TerminalDO authenticatedTerminal, AuthorizationTokenDTO authToken)
        {
            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            await
                restClient.PostAsync<object>(
                    new Uri(authenticatedTerminal.Endpoint + "/terminals/" + authenticatedTerminal.Name + "/events"), new { fr8_user_id = userId, auth_token = authToken });
        }

    }
}