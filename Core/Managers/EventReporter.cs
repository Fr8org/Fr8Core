using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Core.Managers.APIManagers.Packagers;
using Core.Interfaces;
using Core.Services;
using StructureMap;
using Utilities;
using Utilities.Logging;
using Data.Interfaces.DataTransferObjects;

//NOTES: Do NOT put Incidents here. Put them in IncidentReporter


namespace Core.Managers
{
    public class EventReporter
    {
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
            EventManager.EventProcessLaunched += LogEventProcessLaunched;
            EventManager.EventProcessNodeCreated += LogEventProcessNodeCreated;
            EventManager.EventCriteriaEvaluationStarted += LogEventCriteriaEvaluationStarted;
            EventManager.EventCriteriaEvaluationFinished += LogEventCriteriaEvaluationFinished;
            EventManager.EventActionStarted += LogEventActionStarted;
            EventManager.EventActionDispatched += LogEventActionDispatched;
            EventManager.PluginEventReported += LogPluginEvent;
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
            EventManager.EventProcessLaunched -= LogEventProcessLaunched;
            EventManager.EventProcessNodeCreated -= LogEventProcessNodeCreated;
            EventManager.EventCriteriaEvaluationStarted -= LogEventCriteriaEvaluationStarted;
            EventManager.EventCriteriaEvaluationFinished -= LogEventCriteriaEvaluationFinished;
            EventManager.EventActionStarted -= LogEventActionStarted;
            EventManager.EventActionDispatched -= LogEventActionDispatched;
            EventManager.PluginEventReported -= LogPluginEvent;
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

        private static void TrackablePropertyUpdated(string entityName, string propertyName, object id,
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
                    CreatedByID = ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser(),
                    Status = value != null ? value.ToString() : null,
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
            }
        }

        private static void EntityStateChanged(string entityName, object id, string stateName, string stateValue)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFactDO = new FactDO
                {
                    PrimaryCategory = entityName,
                    SecondaryCategory = stateName,
                    Activity = "StateChanged",
                    ObjectId = id != null ? id.ToString() : null,
                    CreatedByID = ObjectFactory.GetInstance<ISecurityServices>().GetCurrentUser(),
                    Status = stateValue,
                };
                uow.FactRepository.Add(newFactDO);
                uow.SaveChanges();
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
                    CustomerId = curUserId,
                    ObjectId = null,
                    Data = string.Format("User with email :{0}, created from: {1}", uow.UserRepository.GetByKey(curUserId).EmailAddress.Address, new StackTrace())
                };

                SaveFact(curAction);
            }
        }

        public void EmailReceived(int emailId, string customerId)
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
                    CustomerId = customerId,
                    ObjectId = emailId.ToString(CultureInfo.InvariantCulture)
                };

                curAction.Data = string.Format("{0} ID :{1}, {2} {3}: ObjectId: {4} EmailAddress: {5} Subject: {6}", curAction.PrimaryCategory, emailId, curAction.SecondaryCategory, curAction.Activity, emailId, (uow.UserRepository.GetByKey(curAction.CustomerId).EmailAddress.Address), emailSubject);

                SaveFact(curAction);
            }
        }

        public void EventBooked(int eventId, string customerId)
        {
            FactDO curAction = new FactDO
            {
                PrimaryCategory = "Event",
                SecondaryCategory = "",
                Activity = "Booked",
                CustomerId = customerId,
                ObjectId = eventId.ToString(CultureInfo.InvariantCulture)
            };
            SaveFact(curAction);
        }
        public void EmailSent(int emailId, string customerId)
        {
            FactDO curAction = new FactDO
            {
                PrimaryCategory = "Email",
                SecondaryCategory = "",
                Activity = "Sent",
                CustomerId = customerId,
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
        //            CustomerId = bookingRequestDO.CustomerID,
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
                CustomerId = userId,
                ObjectId = null,
                Data = string.Format("EnvelopeId: {0}.",
                        envelopeId)
            };
            LogFactInformation(fact, "DocusignNotificationReceived");
            SaveFact(fact);
        }

        /// <summary>
        /// The method logs the fact of Process Template creation.      
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="processTemplateId">EnvelopeId received from DocuSign.</param>
        public void ProcessTemplateCreated(string userId, string processTemplateName)
        {
            FactDO fact = new FactDO
            {
                PrimaryCategory = "ProcessTemplateService",
                SecondaryCategory = null,
                Activity = "Created",
                CustomerId = userId,
                ObjectId = "0",
                Data = string.Format("ProcessTemplate Name: {0}.",
                        processTemplateName)
            };
            LogFactInformation(fact, "ProcessTemplateCreated");
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
                PrimaryCategory = "Notification",
                Activity = "Received",
                Data = message
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(fact);
                uow.SaveChanges();
            }
            LogFactInformation(fact, "ImproperDocusignNotificationReceived", EventType.Warning);
        }

        /// <summary>
        /// The method records information about unhandled exceptions. 
        /// </summary>
        /// <param name="message"></param>
        public void UnhandledErrorCaught(string message)
        {
            var incidentDO = new IncidentDO
            {
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
        public void AlertProcessProcessing(string userId, string envelopeId, int processId)
        {
            FactDO fact = new FactDO
            {
                PrimaryCategory = "Notification",
                SecondaryCategory = null,
                Activity = "Processed",
                CustomerId = userId,
                ObjectId = null,
                Data = string.Format("A notification from DocuSign is processed. UserId: {0}, EnvelopeId: {1}, ProcessDO id: {2}.",
                        userId,
                        envelopeId,
                        processId)
            };
            LogFactInformation(fact, "ProcessProcessing");
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
            LogFactInformation(fact, fact.SecondaryCategory + " " + fact.Activity);
        }

        public void UserRegistered(DockyardAccountDO curUser)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO curFactDO = new FactDO
                {
                    PrimaryCategory = "User",
                    SecondaryCategory = "",
                    Activity = "Registered",
                    CustomerId = curUser.Id,
                    ObjectId = null,
                    Data = string.Format("User registrated with :{0},", curUser.EmailAddress.Address)
                    //Data = "User registrated with " + curUser.EmailAddress.Address
                };
                Logger.GetLogger().Info(curFactDO.Data);
                uow.FactRepository.Add(curFactDO);
                uow.SaveChanges();
            }
        }

        //Do we need/use both this and the immediately preceding event? 
        //public void BookingRequestOwnershipChanged(int bookingRequestId, string bookerId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
        //        if (bookingRequestDO == null)
        //            throw new EntityNotFoundException<BookingRequestDO>(bookingRequestId);
        //        var bookerDO = uow.UserRepository.GetByKey(bookerId);
        //        if (bookerDO == null)
        //            throw new EntityNotFoundException<UserDO>(bookerId);
        //        string status = bookingRequestDO.BookingRequestStateTemplate.Name;
        //        FactDO curAction = new FactDO
        //        {
        //            PrimaryCategory = "BookingRequest",
        //            SecondaryCategory = "Ownership",
        //            Activity = "Change",
        //            CustomerId = bookingRequestDO.Customer.Id,
        //            ObjectId = bookingRequestDO.Id.ToString(CultureInfo.InvariantCulture),
        //            BookerId = bookerId,
        //            Status = status,
        //            Data = string.Format(
        //                    "BookingRequest ID :{0}, Booker EmailAddress: {1}",
        //                    bookingRequestDO.Id,
        //                    bookerDO.EmailAddress.Address)
        //        };

        //        //AddFact(uow, curAction);
        //        uow.SaveChanges();
        //    }
        //}

        private void AddFactOnToken(string userId, string activity)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO factDO = new FactDO
                {
                    PrimaryCategory = "DocuSign",
                    SecondaryCategory = "Token",
                    Activity = activity,
                    CustomerId = userId,
                };

                uow.FactRepository.Add(factDO);
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Logs fact information using the standard log mechanisms.
        /// </summary>
        /// <param name="fact">An instance of FactDO class.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="eventType">Event type.</param>
        private void LogFactInformation(HistoryItemDO fact, string eventName, EventType eventType = EventType.Info)
        {
            string message = string.Format(
                "Event {0} generated with CustomerId = {1}, ObjectId = {2} and Data = {3}.",
                eventName,
                fact.CustomerId,
                fact.ObjectId,
                fact.Data);

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
                CustomerId = null,
                Data = "DocuSign Notificaiton Received",
                ObjectId = null,
                PrimaryCategory = "External Event",
                SecondaryCategory = "DocuSign",
                Activity = "Received"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventProcessLaunched(ProcessDO launchedProcess)
        {
            var fact = new FactDO
            {
                CustomerId = launchedProcess.DockyardAccountId,
                Data = launchedProcess.Id.ToStr(),
                ObjectId = launchedProcess.Id.ToStr(),
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Process",
                Activity = "Launched"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventProcessNodeCreated(ProcessNodeDO processNode)
        {
            ProcessDO processInExecution;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processInExecution = uow.ProcessRepository.GetByKey(processNode.ParentProcessId);
            }

            var fact = new FactDO
            {
                CustomerId = processInExecution.DockyardAccountId,
                Data = processInExecution.Id.ToStr(),
                ObjectId = processNode.Id.ToStr(),
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Process Node",
                Activity = "Created"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventCriteriaEvaluationStarted(int processId)
        {
            ProcessDO processInExecution;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processInExecution = uow.ProcessRepository.GetByKey(processId);
            }

            var fact = new FactDO
            {
                CustomerId = processInExecution.DockyardAccountId,
                Data = processInExecution.Id.ToStr(),
                ObjectId = null,
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Criteria Evaluation",
                Activity = "Started"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventCriteriaEvaluationFinished(int curProcessId)
        {
            ProcessDO processInExecution;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                processInExecution = uow.ProcessRepository.GetByKey(curProcessId);
            }

            var fact = new FactDO
            {
                CustomerId = processInExecution.DockyardAccountId,
                Data = processInExecution.Id.ToStr(),
                ObjectId = null,
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Criteria Evaluation",
                Activity = "Finished"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventActionStarted(ActionDO curAction)
        {
            ProcessDO processInExecution;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                int? processId = uow.ActionListRepository.GetByKey(curAction.ParentActionListId).ProcessID;
                processInExecution = uow.ProcessRepository.GetByKey(processId);
            }

            var fact = new FactDO
            {
                CustomerId = processInExecution.DockyardAccountId,
                Data = processInExecution.Id.ToStr(),
                ObjectId = curAction.Id.ToStr(),
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Action",
                Activity = "Started"
            };

            SaveAndLogFact(fact);
        }

        private void LogEventActionDispatched(ActionPayloadDTO curAction)
        {
            ProcessDO processInExecution;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                int? processId = uow.ActionListRepository.GetByKey(curAction.ActionListId).ProcessID;
                processInExecution = uow.ProcessRepository.GetByKey(processId);
            }

            var fact = new FactDO
            {
                CustomerId = processInExecution.DockyardAccountId,
                Data = processInExecution.Id.ToStr(),
                ObjectId = curAction.Id.ToStr(),
                PrimaryCategory = "Process Execution",
                SecondaryCategory = "Action",
                Activity = "Dispatched"
            };

            SaveAndLogFact(fact);
        }

        private void LogPluginEvent(EventData eventData)
        {
            var fact = new FactDO
            {
                ObjectId = eventData.ObjectId,
                CustomerId = eventData.CustomerId,
                Data = eventData.Data,
                PrimaryCategory = eventData.PrimaryCategory,
                SecondaryCategory = eventData.SecondaryCategory,
                Activity = eventData.Activity
            };

            SaveAndLogFact(fact);
        }


        private enum EventType
        {
            Info,
            Error,
            Warning
        }
    }
}