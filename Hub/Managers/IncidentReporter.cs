using System;
using Data.Entities;
using StructureMap;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Interfaces;
using Hub.Services;

namespace Hub.Managers
{
    public class IncidentReporter
    {
        private readonly EventReporter _eventReporter;
        private readonly ITerminal _terminal;
        private readonly ISecurityServices _sercurity;

        public IncidentReporter(EventReporter eventReporter, ITerminal terminal, ISecurityServices securityService)
        {
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
            EventManager.IncidentOAuthAuthenticationFailed += OAuthAuthenticationFailed;
            EventManager.KeyVaultFailure += KeyVaultFailure;
            EventManager.EventAuthTokenSilentRevoke += AuthTokenSilentRevoke;
            EventManager.EventContainerFailed += ContainerFailed;
            EventManager.EventUnexpectedError += UnexpectedError;
            EventManager.PlanActivationFailedEvent += PlanActivationFailed;
            EventManager.EventTokenValidationFailed += TokenValidationFailed;
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

        private void ProcessIncidentTerminalActivityActivationFailed(string terminalUrl, string additionalData, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = terminalUrl + " [ " + additionalData  + " ] ",
                ObjectId = objectId, // in this case objectId is ActivityId
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
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
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

        private void ProcessIncidentTerminalConfigureFailed(string curTerminalUrl, string additionalData, string errorMessage, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curTerminalUrl + "[" + additionalData+" ] " + errorMessage,
                ObjectId = objectId, // in this case objectId is ActivityId
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Configure",
                Component = "Hub",
                Activity = "Configuration Failed"
            };
            SaveAndLogIncident(incident);
        }

        private void ProcessIncidentTerminalInternalFailureOccurred(string curTerminalUrl, string additionalData, Exception e, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curTerminalUrl + $"[ {additionalData} ]  Message =  [ {e} ]",
                ObjectId = objectId, // in this case objectId is ActivityId
                PrimaryCategory = "Terminal",
                SecondaryCategory = "Internal",
                Component = "Terminal",
                Activity = "Configuration Failed"
            };

            // Database is not available from a terminal web application
            // so only log incidents 
            LogIncident(incident);
        }

        private void ProcessIncidentTerminalRunFailed(string curTerminalUrl, string additionalData, string errorMessage, string objectId)
        {
            var incident = new IncidentDO
            {
                Fr8UserId = _sercurity.GetCurrentUser(),
                Data = curTerminalUrl + " [ " + additionalData+ " ] " + errorMessage,
                ObjectId = objectId, // in this case objectId is ActivityId
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

        private void LogTerminalIncident(LoggingDataCM incidentItem)
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

        private void GenerateLogData(HistoryItemDO currentIncident)
        {
            var logData = string.Format("{0} {1} {2}:" + " ObjectId: {3} CustomerId: {4}",
                currentIncident.PrimaryCategory,
                currentIncident.SecondaryCategory,
                currentIncident.Activity,
                currentIncident.ObjectId,
                currentIncident.Fr8UserId);

            //Logger.GetLogger().Info(logData);
            Logger.GetLogger().Info(logData);
        }

        private void AlertManagerOnAlertResponseReceived(int bookingRequestId, string userId, string customerId)
        {
            using (var _uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var incidentDO = new IncidentDO
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "Response Received",
                    Fr8UserId = customerId,
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
                var incidentDO = new IncidentDO
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
                var incidentDO = new IncidentDO
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
        }

        public void ReportUserRegistrationError(Exception ex)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var incidentDO = new IncidentDO
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
                var incidentDO = new IncidentDO
                {
                    PrimaryCategory = "BookingRequest",
                    SecondaryCategory = "BookerAction",
                    Activity = "MergedBRs",
                    ObjectId = originalBRId.ToString()
                };

                var logData = string.Format("{0} {1} {2}: ",
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
            Logger.GetLogger().Error(errorMessage);
            SaveAndLogIncident(incident);
        }
    }
}
