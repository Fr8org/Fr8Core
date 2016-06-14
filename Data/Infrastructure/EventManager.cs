//We rename .NET style "events" to "alerts" to avoid confusion with our business logic Alert concepts
using System;
using Data.Entities;
using System.Data.Entity.Infrastructure;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Data.Infrastructure
{
    //this class serves as both a registry of all of the defined alerts as well as a utility class.
    public static class EventManager
    {
        public delegate void UnexpectedErrorHandler(Exception ex);
        public static event UnexpectedErrorHandler EventUnexpectedError;

        public delegate void ResponseRecievedHandler(int bookingRequestId, String bookerID, String customerID);
        public static event ResponseRecievedHandler AlertResponseReceived;

        public delegate void TrackablePropertyUpdatedHandler(string name, string contextTable, object id, object status);
        public static event TrackablePropertyUpdatedHandler AlertTrackablePropertyUpdated;

        public delegate void EntityStateChangedHandler(string entityName, object id, string stateName, string stateValue);
        public static event EntityStateChangedHandler AlertEntityStateChanged;

        public delegate void IncidentTerminalConfigurePOSTFailureHandler(string terminalUrl, string curActionDTO, string errorMessage, string objectId);
        public static event IncidentTerminalConfigurePOSTFailureHandler IncidentTerminalConfigureFailed;

        public delegate void IncidentTerminalRunPOSTFailureHandler(string terminalUrl, string curActionDTO, string errorMessage, string objectId);
        public static event IncidentTerminalRunPOSTFailureHandler IncidentTerminalRunFailed;

        public delegate void IncidentTerminalInternalFailureHandler(string terminalUrl, string containerId, Exception e, string objectId);
        public static event IncidentTerminalInternalFailureHandler IncidentTerminalInternalFailureOccurred;

        public delegate void IncidentTerminalActionActivationPOSTFailureHandler(string terminalUrl, string additionalData, string objectId);
        public static event IncidentTerminalActionActivationPOSTFailureHandler IncidentTerminalActionActivationFailed;

        public delegate void TerminalActionActivatedHandler(ActivityDO activity);
        public static event TerminalActionActivatedHandler TerminalActionActivated;

        public delegate void PlanActivationFailedHandler(PlanDO plan, string reason);
        public static event PlanActivationFailedHandler PlanActivationFailedEvent;

        public delegate void ExplicitCustomerCreatedHandler(string curUserId);
        public static event ExplicitCustomerCreatedHandler AlertExplicitCustomerCreated;

        public delegate void CustomerCreatedHandler(Fr8AccountDO user);
        public static event CustomerCreatedHandler AlertCustomerCreated;

        public delegate void EmailReceivedHandler(int emailId, string customerId);
        public static event EmailReceivedHandler AlertEmailReceived;

        public delegate void EventBookedHandler(int eventId, string customerId);
        public static event EventBookedHandler AlertEventBooked;

        public delegate void EmailSentHandler(int emailId, string customerId);
        public static event EmailSentHandler AlertEmailSent;

        public delegate void EmailProcessingHandler(string dateReceived, string errorMessage);
        public static event EmailProcessingHandler AlertEmailProcessingFailure;

        public delegate void IncidentOAuthAuthenticationFailedHandler(string requestQueryString, string errorMessage);
        public static event IncidentOAuthAuthenticationFailedHandler IncidentOAuthAuthenticationFailed;

        public delegate void UserRegistrationHandler(Fr8AccountDO curUser);
        public static event UserRegistrationHandler AlertUserRegistration;

        public delegate void Fr8AccountTerminalRegistrationHandler(TerminalDO terminalDO);
        public static event Fr8AccountTerminalRegistrationHandler AlertFr8AccountTerminalRegistration;

        public delegate void UserRegistrationErrorHandler(Exception ex);
        public static event UserRegistrationErrorHandler AlertUserRegistrationError;

        public delegate void Error_EmailSendFailureHandler(int emailId, string message);
        public static event Error_EmailSendFailureHandler AlertError_EmailSendFailure;

        //public delegate void ErrorSyncingCalendarHandler(IRemoteCalendarAuthDataDO authData, IRemoteCalendarLinkDO calendarLink = null);
        //public static event ErrorSyncingCalendarHandler AlertErrorSyncingCalendar;

        public delegate void HighPriorityIncidentCreatedHandler(int incidentId);
        public static event HighPriorityIncidentCreatedHandler AlertHighPriorityIncidentCreated;

        public delegate void UserNotificationHandler(string userId, string message, TimeSpan expiresIn = default(TimeSpan));
        public static event UserNotificationHandler AlertUserNotification;

        //public delegate void BookingRequestMergedHandler(int originalBRId, int targetBRId);
        //public static event BookingRequestMergedHandler AlertBookingRequestMerged;

        //EventProcessRequestReceived 
        public delegate void EventProcessRequestReceivedHandler(ContainerDO containerId);
        public static event EventProcessRequestReceivedHandler EventProcessRequestReceived;

        public delegate void OAuthEventHandler(string userId);
        public static event OAuthEventHandler AlertTokenRequestInitiated;
        public static event OAuthEventHandler AlertTokenObtained;
        public static event OAuthEventHandler AlertTokenRevoked;

        public delegate void TerminalIncidentHandler(LoggingDataCM incidentItem);
        public static event TerminalIncidentHandler TerminalIncidentReported;

        public delegate void EventContainerLaunchedHandler(ContainerDO launchedContainer);
        public static event EventContainerLaunchedHandler EventContainerLaunched;

        public delegate void EventContainerFailedHandler(PlanDO plan, Exception ex, string containerId);
        public static event EventContainerFailedHandler EventContainerFailed;

        public delegate void EventContainerCreatedHandler(ContainerDO containerDO);
        public static event EventContainerCreatedHandler EventContainerCreated;

        public delegate void EventContainerSentHandler(ContainerDO containerDO, ActivityDO activityDO);
        public static event EventContainerSentHandler EventContainerSent;

        public delegate void EventContainerReceivedHandler(ContainerDO containerDO, ActivityDO activityDO);
        public static event EventContainerReceivedHandler EventContainerReceived;

        public delegate void EventContainerStateChangedHandler(DbPropertyValues currentValues);
        public static event EventContainerStateChangedHandler EventContainerStateChanged;

        public delegate void EventCriteriaEvaluationStartedHandler(Guid processId);
        public static event EventCriteriaEvaluationStartedHandler EventCriteriaEvaluationStarted;

        public delegate void EventCriteriaEvaluationFinishedHandler(Guid processId);
        public static event EventCriteriaEvaluationFinishedHandler EventCriteriaEvaluationFinished;

        public delegate void EventActionStartedHandler(ActivityDO activity, ContainerDO container);
        public static event EventActionStartedHandler EventActionStarted;

        public delegate void EventActionDispatchedHandler(ActivityDO curActivity, Guid processId);
        public static event EventActionDispatchedHandler EventActionDispatched;

        public delegate void TerminalEventHandler(LoggingDataCM eventDataCm);
        public static event TerminalEventHandler TerminalEventReported;

        public delegate void EventTwilioSMSSentHandler(string number, string message);
        public static event EventTwilioSMSSentHandler EventTwilioSMSSent;

        public delegate void IncidentTwilioSMSSendFailureHandler(string number, string message, string errorMsg);
        public static event IncidentTwilioSMSSendFailureHandler IncidentTwilioSMSSendFailure;

        public delegate object AuthenticationCompletedEventHandler(string userId, TerminalDO authenticatedTerminal, AuthorizationTokenDTO authToken);
        public static event AuthenticationCompletedEventHandler EventAuthenticationCompleted;

        public delegate void KeyVaultFailureHandler(string keyVaultMethod, Exception ex);
        public static event KeyVaultFailureHandler KeyVaultFailure;

        public delegate void AuthTokenSilentRevokeHandler(AuthorizationTokenDO authToken);
        public static event AuthTokenSilentRevokeHandler EventAuthTokenSilentRevoke;

        public delegate void AuthTokenCreatedHandler(AuthorizationTokenDO authToken);
        public static event AuthTokenCreatedHandler EventAuthTokenCreated;

        public delegate void AuthTokenRemovedHandler(AuthorizationTokenDO authToken);
        public static event AuthTokenRemovedHandler EventAuthTokenRemoved;

        public delegate void PlanActivatedHandler(Guid planId);
        public static event PlanActivatedHandler EventPlanActivated;

        public delegate void PlanDeactivatedHandler(Guid planId);
        public static event PlanDeactivatedHandler EventPlanDeactivated;

        public delegate void ContainerExecutionCompleteHandler(ContainerDO containerDO);
        public static event ContainerExecutionCompleteHandler EventContainerExecutionCompleted;

        public delegate void ActivityRunRequestedHandler(ActivityDO activityDo, ContainerDO containerDO);
        public static event ActivityRunRequestedHandler EventActivityRunRequested;


        public delegate void ActivityResponseReceivedHandler(ActivityDO activityDo, ActivityResponse responseType);
        public static event ActivityResponseReceivedHandler EventActivityResponseReceived;


        public delegate void ProcessingTerminatedPerActivityResponseHandler(ContainerDO containerDO, ActivityResponse resposneType);
        public static event ProcessingTerminatedPerActivityResponseHandler EventProcessingTerminatedPerActivityResponse;

        public delegate void TokenValidationFailedHandler(string token, string errorMessage);
        public static event TokenValidationFailedHandler EventTokenValidationFailed;

        #region Method

        public static void PlanActivated(Guid planId)
        {
            var handler = EventPlanActivated;
            if (handler != null) handler(planId);
        }

        public static void PlanDeactivated(Guid planId)
        {
            var handler = EventPlanDeactivated;
            if (handler != null) handler(planId);
        }

        public static void ContainerExecutionCompleted(ContainerDO containerDO)
        {
            var handler = EventContainerExecutionCompleted;
            if (handler != null) handler(containerDO);
        }

        public static void ActivityRunRequested(ActivityDO activityDo, ContainerDO containerDO)
        {
            var handler = EventActivityRunRequested;
            if (handler != null) handler(activityDo, containerDO);
        }

        public static void ActivityResponseReceived(ActivityDO activityDo, ActivityResponse responseType)
        {
            var handler = EventActivityResponseReceived;
            if (handler != null) handler(activityDo, responseType);
        }

        public static void ProcessingTerminatedPerActivityResponse(ContainerDO containerDO, ActivityResponse resposneType)
        {
            var handler = EventProcessingTerminatedPerActivityResponse;
            if (handler != null) handler(containerDO, resposneType);
        }

        public static void UnexpectedError(Exception ex)
        {
            var handler = EventUnexpectedError;

            if (handler != null)
            {
                handler.Invoke(ex);
            }
        }

        public static void AuthTokenSilentRevoke(AuthorizationTokenDO authToken)
        {
            var handler = EventAuthTokenSilentRevoke;

            if (handler != null)
            {
                handler.Invoke(authToken);
            }
        }

        public static void AuthTokenCreated(AuthorizationTokenDO authToken)
        {
            var handler = EventAuthTokenCreated;

            if (handler != null)
            {
                handler.Invoke(authToken);
            }
        }

        public static void AuthTokenRemoved(AuthorizationTokenDO authToken)
        {
            var handler = EventAuthTokenRemoved;

            if (handler != null)
            {
                handler.Invoke(authToken);
            }
        }

        public static void KeyVaultFailed(string keyVaultMethod, Exception ex)
        {
            var handler = KeyVaultFailure;

            if (handler != null)
            {
                handler.Invoke(keyVaultMethod, ex);
            }
        }

        public static void TerminalConfigureFailed(string terminalUrl, string additionalData, string errorMessage, string objectId)
        {
            IncidentTerminalConfigurePOSTFailureHandler handler = IncidentTerminalConfigureFailed;
            if (handler != null) handler(terminalUrl, additionalData, errorMessage, objectId);
        }

        public static void TerminalRunFailed(string terminalUrl, string additionalData, string errorMessage, string objectId)
        {
            IncidentTerminalRunPOSTFailureHandler handler = IncidentTerminalRunFailed;
            if (handler != null) handler(terminalUrl, additionalData, errorMessage, objectId);
        }

        public static void TerminalInternalFailureOccurred(string terminalUrl, string additionalData, Exception e, string objectId)
        {
            IncidentTerminalInternalFailureHandler handler = IncidentTerminalInternalFailureOccurred;
            if (handler != null) handler(terminalUrl, additionalData, e, objectId);
        }

        public static void TerminalActionActivationFailed(string terminalUrl, string additional, string errorMessage, string objectId)
        {
            IncidentTerminalActionActivationPOSTFailureHandler handler = IncidentTerminalActionActivationFailed;
            if (handler != null) handler(terminalUrl, additional, objectId);
        }

        public static void PlanActivationFailed(PlanDO plan, string reason)
        {
            var handler = PlanActivationFailedEvent;
            if (handler != null) handler(plan, reason);
        }

        public static void UserNotification(string userid, string message, TimeSpan expiresIn = default(TimeSpan))
        {
            UserNotificationHandler handler = AlertUserNotification;
            if (handler != null) handler(userid, message, expiresIn);
        }

        public static void ReportTerminalIncident(LoggingDataCM incidentItem)
        {
            TerminalIncidentHandler handler = TerminalIncidentReported;
            if (handler != null) handler(incidentItem);
        }

        public static void ResponseReceived(int bookingRequestId, String bookerID, String customerID)
        {
            if (AlertResponseReceived != null)
                AlertResponseReceived(bookingRequestId, bookerID, customerID);
        }

        public static void EntityStateChanged(string entityName, object id, string stateName, string stateValue)
        {
            if (AlertEntityStateChanged != null)
                AlertEntityStateChanged(entityName, id, stateName, stateValue);
        }

        public static void TrackablePropertyUpdated(string entityName, string propertyName, object id, object value)
        {
            if (AlertTrackablePropertyUpdated != null)
                AlertTrackablePropertyUpdated(entityName, propertyName, id, value);
        }

        /// <summary>
        /// Publish Customer Created event
        /// </summary>
        public static void ExplicitCustomerCreated(string curUserId)
        {
            if (AlertExplicitCustomerCreated != null)
                AlertExplicitCustomerCreated(curUserId);
        }

        public static void CustomerCreated(Fr8AccountDO user)
        {
            if (AlertCustomerCreated != null)
                AlertCustomerCreated(user);
        }

        public static void EmailReceived(int emailId, string customerId)
        {
            if (AlertEmailReceived != null)
                AlertEmailReceived(emailId, customerId);
        }
        public static void EventBooked(int eventId, string customerId)
        {
            if (AlertEventBooked != null)
                AlertEventBooked(eventId, customerId);
        }
        public static void EmailSent(int emailId, string customerId)
        {
            if (AlertEmailSent != null)
                AlertEmailSent(emailId, customerId);
        }

        public static void EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            if (AlertEmailProcessingFailure != null)
                AlertEmailProcessingFailure(dateReceived, errorMessage);
        }

        public static void UserRegistration(Fr8AccountDO curUser)
        {
            if (AlertUserRegistration != null)
                AlertUserRegistration(curUser);
        }

        public static void UserRegistrationError(Exception ex)
        {
            UserRegistrationErrorHandler handler = AlertUserRegistrationError;
            if (handler != null) handler(ex);
        }

        public static void Fr8AccountTerminalRegistration(TerminalDO terminalDO)
        {
            if (AlertFr8AccountTerminalRegistration != null)
                AlertFr8AccountTerminalRegistration(terminalDO);
        }

        public static void Error_EmailSendFailure(int emailId, string message)
        {
            if (AlertError_EmailSendFailure != null)
                AlertError_EmailSendFailure(emailId, message);
        }

        public static void HighPriorityIncidentCreated(int incidentId)
        {
            HighPriorityIncidentCreatedHandler handler = AlertHighPriorityIncidentCreated;
            if (handler != null) handler(incidentId);
        }

        public static void TokenRequestInitiated(string userId)
        {
            var handler = AlertTokenRequestInitiated;
            if (handler != null) handler(userId);
        }

        public static void TokenObtained(string userId)
        {
            var handler = AlertTokenObtained;
            if (handler != null) handler(userId);
        }

        public static void TokenRevoked(string userId)
        {
            var handler = AlertTokenRevoked;
            if (handler != null) handler(userId);
        }

        public static void ContainerLaunched(ContainerDO launchedContainer)
        {
            var handler = EventContainerLaunched;
            if (handler != null) handler(launchedContainer);
        }

        public static void ContainerFailed(PlanDO plan, Exception ex, string containerId)
        {
            var handler = EventContainerFailed;
            if (handler != null) handler(plan, ex, containerId);
        }

        public static void CriteriaEvaluationStarted(Guid processId)
        {
            var handler = EventCriteriaEvaluationStarted;
            if (handler != null) handler(processId);
        }

        public static void CriteriaEvaluationFinished(Guid processId)
        {
            var handler = EventCriteriaEvaluationFinished;
            if (handler != null) handler(processId);
        }

        public static void ActionStarted(ActivityDO activity, ContainerDO container)
        {
            var handler = EventActionStarted;
            if (handler != null) handler(activity, container);
        }

        public static void ActionDispatched(ActivityDO curActivity, Guid processId)
        {
            var handler = EventActionDispatched;
            if (handler != null) handler(curActivity, processId);
        }

        public static void ReportTerminalEvent(LoggingDataCM eventDataCm)
        {
            TerminalEventHandler handler = TerminalEventReported;
            if (handler != null) handler(eventDataCm);
        }

        public static void OAuthAuthenticationFailed(string requestQueryString, string errorMessage)
        {
            var handler = IncidentOAuthAuthenticationFailed;
            if (handler != null) handler(requestQueryString, errorMessage);
        }

        public static void ActionActivated(ActivityDO activity)
        {
            var handler = TerminalActionActivated;
            if (handler != null) handler(activity);
        }

        public static void ProcessRequestReceived(ContainerDO containerDO)
        {
            var handler = EventProcessRequestReceived;
            if (handler != null) handler(containerDO);
        }

        public static void TwilioSMSSent(string number, string message)
        {
            var handler = EventTwilioSMSSent;
            if (handler != null) handler(number, message);
        }

        public static void TwilioSMSSendFailure(string number, string message, string errorMsg)
        {
            var handler = IncidentTwilioSMSSendFailure;
            if (handler != null) handler(number, message, errorMsg);
        }

        public static void ContainerCreated(ContainerDO containerDO)
        {
            var handler = EventContainerCreated;
            if (handler != null) handler(containerDO);
        }

        public static void ContainerSent(ContainerDO containerDO, ActivityDO activityDO)
        {
            var handler = EventContainerSent;
            if (handler != null) handler(containerDO, activityDO);
        }

        public static void ContainerReceived(ContainerDO containerDO, ActivityDO activityDO)
        {
            var handler = EventContainerReceived;
            if (handler != null) handler(containerDO, activityDO);
        }
        internal static void ContainerStateChanged(DbPropertyValues currentValues)
        {
            var handler = EventContainerStateChanged;
            if (handler != null) handler(currentValues);
        }

        public static void TerminalAuthenticationCompleted(string userId, TerminalDO authenticatedTerminal, AuthorizationTokenDTO authToken)
        {
            var handler = EventAuthenticationCompleted;
            if (handler != null) handler(userId, authenticatedTerminal, authToken);
        }

        public static void TokenValidationFailed(string token, string errorMessage)
        {
            var handler = EventTokenValidationFailed;
            if (handler != null) handler(token, errorMessage);
        }

        #endregion
    }

}