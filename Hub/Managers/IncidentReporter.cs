using System;
using Data.Entities;
using StructureMap;
using Data.Exceptions;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;
using Utilities.Configuration.Azure;
using Utilities.Logging;

namespace Hub.Managers
{
    public class IncidentReporter
    {
        private readonly EventReporter _eventReporter;
        private readonly ITerminal _terminal;
        private readonly ISecurityServices _sercurity;
        private readonly IActivityTemplate _activityTemplate;

        public IncidentReporter(EventReporter eventReporter, ITerminal terminal, ISecurityServices securityService, IActivityTemplate activityTemplate)
        {
            _activityTemplate = activityTemplate;
            _eventReporter = eventReporter;
            _terminal = terminal;
            _sercurity = securityService;
        }

        public void SubscribeToAlerts()
        {
            EventManager.AlertEmailProcessingFailure += ProcessAlert_EmailProcessingFailure;
            EventManager.IncidentTerminalConfigureFailed += ProcessIncidentTerminalConfigureFailed;
            EventManager.IncidentTerminalRunFailed += ProcessIncidentTerminalRunFailed;
            EventManager.AlertError_EmailSendFailure += ProcessEmailSendFailure;
            EventManager.IncidentTerminalActionActivationFailed += ProcessIncidentTerminalActivityActivationFailed;
            EventManager.IncidentTerminalInternalFailureOccurred += ProcessIncidentTerminalInternalFailureOccurred;
            EventManager.AlertResponseReceived += AlertManagerOnAlertResponseReceived;
            EventManager.AlertUserRegistrationError += ReportUserRegistrationError;
            EventManager.TerminalIncidentReported += LogTerminalIncident;
            EventManager.IncidentDocuSignFieldMissing += IncidentDocuSignFieldMissing;
            EventManager.IncidentOAuthAuthenticationFailed += OAuthAuthenticationFailed;
            EventManager.KeyVaultFailure += KeyVaultFailure;
            EventManager.EventAuthTokenSilentRevoke += AuthTokenSilentRevoke;
            EventManager.EventContainerFailed += ContainerFailed;
            EventManager.EventUnexpectedError += UnexpectedError;
            EventManager.PlanActivationFailedEvent += PlanActivationFailed;
            EventManager.EventMultipleMonitorAllDocuSignEventsPlansPerAccountArePresent += EventManager_EventMultipleMonitorAllDocuSignEventsPlansPerAccountArePresent;
            EventManager.EventTokenValidationFailed += TokenValidationFailed;
        }

        public void EventManager_EventMultipleMonitorAllDocuSignEventsPlansPerAccountArePresent(string external_email)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = string.Join(
                   "Multiple Monitor_All_DocuSign_Events plans were created for one DocuSign account: ", external_email
               ),
                PrimaryCategory = "Error",
                SecondaryCategory = "Unexpected",
                Component = "Terminal",
                Activity = "Unexpected Error"
            };

            SaveAndLogIncident(incident);
        }

        private void UnexpectedError(Exception ex)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = string.Join(
                    "Unexpected error: ",
                    ex.Message,
                    ex.StackTrace ?? ""
                ),
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Authentication",
                Component = "Hub",
                Activity = "Unexpected Error"
            };

            SaveAndLogIncident(incident);
        }

        private void PlanActivationFailed(PlanDO plan, string reason)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = "unknown",
                Data = "Plan activation failed, plan.Id = " + plan.Id.ToString()
                    + ", plan.Name = " + plan.Name
                    + ", plan.PlanState = " + plan.PlanState.ToString()
                    + ", reason = " + reason,
                PrimaryCategory = "Plan",
                SecondaryCategory = "Activation",
                Component = "Hub",
                Activity = "Plan Activation"
            };

            SaveAndLogIncident(incident);
        }

        private void KeyVaultFailure(string keyVaultMethod, Exception ex)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = Environment.NewLine + $"KeyVault Uri: {CloudConfigurationManager.GetSetting("KeyVaultUrl")}, Client Id: {CloudConfigurationManager.GetSetting("KeyVaultClientId")} Method: {keyVaultMethod}. Reason: {ex.Message}. StackTrace: {ex.StackTrace ?? ""}",
                PrimaryCategory = "KeyVault",
                SecondaryCategory = "QuerySecurePartAsync",
                Component = "Hub",
                Activity = "KeyVault Failed"
            };

            SaveAndLogIncident(incident);
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

        private void AuthTokenSilentRevoke(AuthorizationTokenDO authToken)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = string.Join(
                    Environment.NewLine,
                    "AuthToken method: Silent Revoke",
                    "User Id: " + authToken.UserID.ToString(),
                    "Terminal name: " + FormatTerminalName(authToken),
                    "External AccountId: " + authToken.ExternalAccountId
                ),
                PrimaryCategory = "AuthToken",
                SecondaryCategory = "Silent Revoke",
                Component = "Hub",
                Activity = "AuthToken Silent Revoke"
            };
            SaveAndLogIncident(incident);
        }

        private void ContainerFailed(PlanDO plan, Exception ex, string containerId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = string.Join(
                    Environment.NewLine,
                    "Container failure.",
                    "PlanName: " + (plan != null ? plan.Name : "unknown"),
                    "PlanId: " + (plan != null ? plan.Id.ToString() : "unknown"),
                    ex.Message,
                    ex.StackTrace ?? ""
                ),
                ObjectId = containerId,
                PrimaryCategory = "Container",
                SecondaryCategory = "Execution",
                Component = "Hub",
                Activity = "Container failure"
            };

            SaveAndLogIncident(incident);
        }

        private void ProcessIncidentTerminalActivityActivationFailed(string terminalUrl, string curActionId, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = terminalUrl + " ActionId = [" + curActionId+"]",
                ObjectId = objectId,
                PrimaryCategory = "Action",
                SecondaryCategory = "Activation",
                Activity = "Completed"
            };
            SaveAndLogIncident(incident);
        }

        /// <summary>
        /// Logs incident information using the standard log mechanisms.
        ///</summary>

        private void SaveAndLogIncident(IncidentDO curIncident)
        {
            SaveIncident(curIncident);
            LogIncident(curIncident);
        }

        private void SaveIncident(IncidentDO curIncident)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(curIncident);
                uow.SaveChanges();
            }
        }

        private void LogIncident(IncidentDO curIncident)
        {
            _eventReporter.LogHistoryItem(curIncident, EventType.Error);
            //_eventReporter.LogFactInformation(curIncident, curIncident.SecondaryCategory + " " + curIncident.Activity, EventReporter.EventType.Error);
        }

        private void ProcessIncidentTerminalConfigureFailed(string curTerminalUrl, string curActionId, string errorMessage, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curTerminalUrl + " ActionId = [" + curActionId + "] " + errorMessage,
                ObjectId = objectId,
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Configure",
                Component = "Hub",
                Activity = "Configuration Failed"
            };
            SaveAndLogIncident(incident);
        }

        private void ProcessIncidentTerminalInternalFailureOccurred(string curTerminalUrl, string containerId, Exception e, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curTerminalUrl + $"ContainerId = [{containerId}]  Message =  [ " + e.ToString() + " ]",
                ObjectId = objectId,
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Internal",
                Component = "Terminal",
                Activity = "Configuration Failed"
            };

            // Database is not available from a terminal web application
            // so only log incidents 
            LogIncident(incident);
        }

        private void ProcessIncidentTerminalRunFailed(string curTerminalUrl, string curActionId, string errorMessage, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curTerminalUrl + "  ActionId = [" + curActionId + "] " + errorMessage,
                ObjectId = objectId,
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Run",
                Component = "Hub",
                Activity = "Configuration Failed"
            };
            SaveAndLogIncident(incident);
        }

        private void OAuthAuthenticationFailed(string curRequestQueryString, string errorMessage)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = "Query string: " + curRequestQueryString + "      \r\n" + errorMessage,
                ObjectId = _sercurity.GetCurrentUser(),
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Authentication",
                Activity = "OAuth Authentication Failed"
            };
            SaveAndLogIncident(incident);
        }

        private void LogTerminalIncident(LoggingDataCm incidentItem)
        {
            var currentIncident = new IncidentDO
            {
                Fr8UserId = incidentItem.Fr8UserId,
                ObjectId = incidentItem.ObjectId,
                Data = incidentItem.Data,
                PrimaryCategory = incidentItem.PrimaryCategory,
                SecondaryCategory = incidentItem.SecondaryCategory,
                Component = "Terminal",
                Activity = incidentItem.Activity
            };
            SaveAndLogIncident(currentIncident);
        }

        private void LogUnparseableNotificationIncident(string curNotificationUrl, string curNotificationPayload)
        {
            var currentIncident = new IncidentDO
            {
                ObjectId = curNotificationPayload,
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curNotificationUrl,
                PrimaryCategory = "Event",
                SecondaryCategory = "External",
                Activity = "Unparseble Notification"
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(currentIncident);
                uow.SaveChanges();

                GenerateLogData(currentIncident);
            }
        }

        private void LogExternalEventReceivedIncident(string curEventPayload)
        {
            var currentIncident = new IncidentDO
            {
                ObjectId = "EventController",
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curEventPayload,
                PrimaryCategory = "Event",
                SecondaryCategory = "External",
                Activity = "Received"
            };

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.IncidentRepository.Add(currentIncident);
                uow.SaveChanges();

                GenerateLogData(currentIncident);
            }
        }

        private void GenerateLogData(HistoryItemDO currentIncident)
        {
            string logData = string.Format("{0} {1} {2}:" + " ObjectId: {3} CustomerId: {4}",
                currentIncident.PrimaryCategory,
                currentIncident.SecondaryCategory,
                currentIncident.Activity,
                currentIncident.ObjectId,
                currentIncident.Fr8UserId);

            //Logger.GetLogger().Info(logData);
            Logger.LogInfo(logData);
        }

        private void ProcessAttendeeUnresponsivenessThresholdReached(int expectedResponseId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var expectedResponseDO = uow.ExpectedResponseRepository.GetByKey(expectedResponseId);
                if (expectedResponseDO == null)
                    throw new EntityNotFoundException<ExpectedResponseDO>(expectedResponseId);
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Negotiation";
                incidentDO.SecondaryCategory = "ClarificationRequest";
                incidentDO.Fr8UserId = expectedResponseDO.UserID;
                incidentDO.ObjectId = expectedResponseId.ToString();
                incidentDO.Activity = "UnresponsiveAttendee";
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
        }

        private void AlertManagerOnAlertResponseReceived(int bookingRequestId, string userID, string customerID)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "Response Received",
                    Fr8UserId = customerID,
                    ObjectId = bookingRequestId.ToString(),
                    Activity = "Response Recieved"
                };
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        public void ProcessAlert_EmailProcessingFailure(string dateReceived, string errorMessage)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO
                {
                    Fr8UserId = _sercurity.GetCurrentUser(),
                    PrimaryCategory = "Email",
                    SecondaryCategory = "Failure",
                    Priority = 5,
                    Activity = "Intake",
                    Data = errorMessage,
                    ObjectId = null
                };
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        }

        private void ProcessEmailSendFailure(int emailId, string message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO
                {
                    Fr8UserId = _sercurity.GetCurrentUser(),
                    PrimaryCategory = "Email",
                    SecondaryCategory = "Failure",
                    Activity = "Send",
                    ObjectId = emailId.ToString(),
                    Data = message
                };
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();
            }
            Email _email = ObjectFactory.GetInstance<Email>();
            //_email.SendAlertEmail("Alert! Kwasant Error Reported: EmailSendFailure",
            //			    string.Format(
            //				  "EmailID: {0}\r\n" +
            //				  "Message: {1}",
            //				  emailId, message));
        }

        public void ReportUserRegistrationError(Exception ex)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO
                {
                    PrimaryCategory = "Fr8Account",
                    SecondaryCategory = "Error",
                    Activity = "Registration",
                    Data = ex.Message
                };
                uow.IncidentRepository.Add(incidentDO);
                uow.SaveChanges();

                GenerateLogData(incidentDO);
            }
        }

        public void BookingRequestMerged(int originalBRId, int targetBRId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "BookerAction",
                    Activity = "MergedBRs",
                    ObjectId = originalBRId.ToString()
                };

                string logData = string.Format("{0} {1} {2}: ",
                        incidentDO.PrimaryCategory,
                        incidentDO.SecondaryCategory,
                        incidentDO.Activity);

                incidentDO.Data = logData + incidentDO.ObjectId;
                uow.IncidentRepository.Add(incidentDO);
                Logger.GetLogger().Info(incidentDO.Data);
                uow.SaveChanges();

                incidentDO.ObjectId = targetBRId.ToString();
                incidentDO.Data = logData + incidentDO.ObjectId;
                uow.IncidentRepository.Add(incidentDO);
                Logger.GetLogger().Info(incidentDO.Data);
                uow.SaveChanges();
            }
        }

        public void IncidentDocuSignFieldMissing(string envelopeId, string fieldName)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO
                {
                    Fr8UserId = _sercurity.GetCurrentUser(),
                    PrimaryCategory = "Envelope",
                    SecondaryCategory = "",
                    ObjectId = envelopeId,
                    Activity = "Action processing",
                    Data =
                        String.Format("IncidentDocuSignFieldMissing: Envelope id: {0}, Field name: {1}", envelopeId,
                            fieldName)
                };
                _uow.IncidentRepository.Add(incidentDO);
                //Logger.GetLogger().Warn(incidentDO.Data);
                Logger.LogWarning(incidentDO.Data);
                _uow.SaveChanges();
            }
        }

        private void TokenValidationFailed(string token, string errorMessage)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = "Token validation failed with error: " + errorMessage,
                ObjectId = _sercurity.GetCurrentUser(),
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Authentication",
                Activity = "Token Validation Failed"
            };
            Logger.LogError(errorMessage);
            SaveAndLogIncident(incident);
        }
    }
}
