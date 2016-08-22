using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Hub.Interfaces;
using System.Data.Entity.Infrastructure;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;

// NOTES: Do NOT put Incidents here. Put them in IncidentReporter
namespace Hub.Managers
{
    public enum EventType
    {
        Info,
        Warning,
        Error
    }

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

        // Register for interesting events
        public void SubscribeToAlerts()
        {
            EventManager.AlertTrackablePropertyUpdated += TrackablePropertyUpdated;
            EventManager.AlertEntityStateChanged += EntityStateChanged;
            EventManager.AlertEmailReceived += EmailReceived;
            EventManager.AlertEventBooked += EventBooked;
            EventManager.AlertEmailSent += EmailSent;
            EventManager.AlertExplicitCustomerCreated += CustomerCreated;

            EventManager.AlertUserRegistration += UserRegistered;

            EventManager.AlertTokenRequestInitiated += OnAlertTokenRequestInitiated;
            EventManager.AlertTokenObtained += OnAlertTokenObtained;
            EventManager.AlertTokenRevoked += OnAlertTokenRevoked;

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
            EventManager.AlertEmailReceived -= EmailReceived;
            EventManager.AlertEventBooked -= EventBooked;
            EventManager.AlertEmailSent -= EmailSent;
            EventManager.AlertExplicitCustomerCreated -= CustomerCreated;

            EventManager.AlertUserRegistration -= UserRegistered;

            EventManager.AlertTokenRequestInitiated -= OnAlertTokenRequestInitiated;
            EventManager.AlertTokenObtained -= OnAlertTokenObtained;
            EventManager.AlertTokenRevoked -= OnAlertTokenRevoked;

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
                Data = string.Join(Environment.NewLine, "Activity Name: " + template?.Name)
            };

            SaveAndLogFact(factDO);
        }

        private void ActivityRunRequested(ActivityDO activityDo, ContainerDO containerDO)
        {
            try
            {
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
                        Data = string.Join(Environment.NewLine, "Activity Name: " + template?.Name)
                    };
                    SaveAndLogFact(factDO);

                    // Create user notifications about activity execution
                    var _pusherNotifier = ObjectFactory.GetInstance<IPusherNotifier>();
                    _pusherNotifier.NotifyUser(new NotificationMessageDTO()
                    {
                        NotificationType = NotificationType.GenericInfo,
                        Subject = "Executing Activity",
                        Message = "For Plan: " + containerDO.Name + "\nContainer: " + containerDO.Id.ToString(),
                        ActivityName = template.Label,
                        Collapsed = true,
                    }, activityDo.Fr8Account.Id);
                }
            }
            catch (Exception exception)
            {
                EventManager.UnexpectedError(exception);
            }
        }

        private void ContainerExecutionCompleted(ContainerDO containerDO)
        {
            var factDO = new FactDO()
            {
                PrimaryCategory = "Container Execution",
                SecondaryCategory = "Container",
                Activity = "Launched",
                ObjectId = containerDO.Id.ToString(),
                Fr8UserId = _security.GetCurrentUser(),
                CreatedByID = _security.GetCurrentUser(),
                Data = string.Join( Environment.NewLine, "Container Id: " + containerDO.Id, "Plan Id: " + containerDO.PlanId )
            };

            SaveAndLogFact(factDO);
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
                Data = string.Join( Environment.NewLine, "Plan State: " + state )
            };
            return factDO;
        }

        private void PlanDeactivated(Guid planId)
        {
            PlanDO planDO = null;
            using (var uowPlan = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                planDO = uowPlan.PlanRepository.GetById<PlanDO>(planId);
            }

            if (planDO != null)
            {
                var factDO = CreatedPlanFact(planId, "Deactivated");
                SaveAndLogFact(factDO);
            }
        }

        private void PlanActivated(Guid planId)
        {
            PlanDO planDO = null;
            using (var uowPlan = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                planDO = uowPlan.PlanRepository.GetById<PlanDO>(planId);
            }

            if (planDO != null)
            {
                var factDO = CreatedPlanFact(planId, "Activated");
                SaveAndLogFact(factDO);
            }
        }

        private void ProcessingTerminatedPerActivityResponse(ContainerDO containerDO, ActivityResponse resposneType)
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
                Data = string.Join( Environment.NewLine, "Container Id: " + containerDO.Name)
            };

            SaveAndLogFact(factDO);
        }

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

            SaveAndLogFact(factDO);
        }

        private void AuthTokenRemoved(AuthorizationTokenDO authToken)
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

            SaveAndLogFact(newFactDO);
        }

        private void TrackablePropertyUpdated(string entityName, string propertyName, object id, object value)
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

            SaveAndLogFact(newFactDO);
        }

        private void EntityStateChanged(string entityName, object id, string stateName, string stateValue)
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

            SaveAndLogFact(newFactDO);
        }

        private void EventManagerOnEventProcessRequestReceived(ContainerDO containerDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(containerDO.PlanId);

                var fact = new FactDO
                {
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

                SaveAndLogFact(curAction);
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

                SaveAndLogFact(curAction);
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

            SaveAndLogFact(curAction);
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

            SaveAndLogFact(curAction);
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
                Data = string.Format("Plan Name: {0}.", planName)
            };
            
            SaveAndLogFact(fact);
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

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(incidentDO);

                //The error may be connected to the fact that DB is unavailable, 
                //we need to be prepared to that. 
                try
                {
                    uow.SaveChanges();
                }
                catch(Exception exp)
                {
                    Logger.GetLogger().Error($"Can`t add incident to repository. Exception = [{exp}]");
                }
                finally
                {
                    LogHistoryItem(incidentDO,EventType.Error);
                }
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
                Data = string.Format("A notification from DocuSign is processed. UserId: {0}, EnvelopeId: {1}, ContainerDO id: {2}.", userId,
                        envelopeId, containerId)
            };

            SaveAndLogFact(fact); 
        }

        private void SaveFact(FactDO curFact)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.FactRepository.Add(curFact);
                uow.MultiTenantObjectRepository.Add(curFact.ToFactCM(), _security.GetCurrentUser());

                uow.SaveChanges();
            }
        }

        private void SaveAndLogFact(FactDO fact)
        {
            SaveFact(fact);
            LogHistoryItem(fact);
        }

        public void UserRegistered(Fr8AccountDO curUser)
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

            SaveAndLogFact(curFactDO);
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

                SaveAndLogFact(curFactDO);
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

            //Logger.GetLogger().Error(message);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(incidentDO);

                //The error may be connected to the fact that DB is unavailable, 
                //we need to be prepared to that. 
                try
                {
                    uow.SaveChanges();
                }
                catch(Exception exp)
                {
                    Logger.GetLogger().Error($"Can`t add incident to repository. Exception = [{exp}]");
                }
                finally
                {
                    LogHistoryItem(incidentDO, EventType.Error);
                }
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

                SaveAndLogFact(factDO);
            }
        }

        public string ComposeOutputString(HistoryItemDO historyItem)
        {
            historyItem = historyItem ?? new HistoryItemDO() { Data = "HistoryItem object is null!" };

            string itemType = historyItem.GetType().Name.Replace("DO", "");

            //trim Data feild if it is too long, max length 256 symbols
            historyItem.Data = historyItem.Data ?? "";
            var dataLen = historyItem.Data.Length > 256 ? 255 : historyItem.Data.Length;
            var substring = historyItem.Data.Substring(0, dataLen);
            substring = dataLen == 255 ? substring + "..." : substring;
            
            //in FactDO we have CreatedById property, so we need crutch to not have Fr8UserId empty
            if (typeof(FactDO) == historyItem.GetType() && historyItem.Fr8UserId.IsNullOrEmpty())
            {
                historyItem.Fr8UserId = (historyItem as FactDO).CreatedByID;
            }

            var message =     $"HistoryItemId = [{historyItem.Id}]; "+
                              $"[{itemType}]: [{historyItem.PrimaryCategory}] " +
                              $"[{historyItem.SecondaryCategory}]" +
                              $"[{historyItem.Activity}], " +
                              $"Data = [{substring}], " +
                              $"Fr8User = [{historyItem.Fr8UserId}], " +
                              $"ObjectId = [{historyItem.ObjectId}]";

            return message;
        }

        /// <summary>
        /// Logs historyItem information using the standard log mechanisms, replacement for LogFactInformation .
        /// </summary>
        /// <param name="historyItem">An instance of FactDO class.</param>
        /// <param name="eventType">Event type.</param>
        public void LogHistoryItem(HistoryItemDO historyItem, EventType eventType = EventType.Info)
        {
            var message = ComposeOutputString(historyItem);
            var logger = Logger.GetLogger();

            if (eventType == EventType.Info)
            {
                logger.Info(message);
            }
            if (eventType == EventType.Warning)
            {
                logger.Warn(message);
            }
            if (eventType == EventType.Error)
            {
                logger.Error(message);
            }

            //Logger.LogMessage(message,eventType);
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

        private void LogTerminalEvent(LoggingDataCM eventDataCm)
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

                SaveAndLogFact(curFact);
            }
        }

        private static async Task PostToTerminalEventsEndPoint(string userId, TerminalDO authenticatedTerminal, AuthorizationTokenDTO authToken)
        {
            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            var terminalService = ObjectFactory.GetInstance<ITerminal>();


            var headers = terminalService.GetRequestHeaders(authenticatedTerminal, userId);

            await restClient.PostAsync<object>(
                    new Uri(authenticatedTerminal.Endpoint + "/terminals/" + authenticatedTerminal.Name + "/events"), new { fr8_user_id = userId, auth_token = authToken }, null, headers);
        }
    }
}