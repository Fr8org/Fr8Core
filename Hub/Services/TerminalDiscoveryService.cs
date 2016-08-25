using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Managers;
using log4net;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Data.States;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Linq.Expressions;
using Hub.Exceptions;
using StructureMap;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;

namespace Hub.Services
{
    public class TerminalDiscoveryService : ITerminalDiscoveryService
    {
        private static readonly ILog Logger = Fr8.Infrastructure.Utilities.Logging.Logger.GetCurrentClassLogger();

        private readonly IActivityTemplate _activityTemplateService;
        private readonly ITerminal _terminal;
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly EventReporter _eventReporter;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly string _serverUrl;
        private readonly ISecurityServices _securityService;

        public TerminalDiscoveryService(
            IActivityTemplate activityTemplateService,
            ITerminal terminal,
            IRestfulServiceClient restfulServiceClient,
            EventReporter eventReporter,
            IUnitOfWorkFactory unitOfWorkFactory,
            IConfigRepository configRepository,
            ISecurityServices securityService)
        {
            _activityTemplateService = activityTemplateService;
            _terminal = terminal;
            _restfulServiceClient = restfulServiceClient;
            _eventReporter = eventReporter;
            _unitOfWorkFactory = unitOfWorkFactory;
            _securityService = securityService;

            var serverProtocol = configRepository.Get("ServerProtocol", String.Empty);
            var domainName = configRepository.Get("ServerDomainName", String.Empty);
            var domainPort = configRepository.Get<int?>("ServerPort", null);

            _serverUrl = $"{serverProtocol}{domainName}{(domainPort == null || domainPort.Value == 80 ? String.Empty : (":" + domainPort.Value))}/";
        }

        public async Task SaveOrRegister(TerminalDTO terminal)
        {
            string curEndpoint = string.Empty;

            // Get the value of TerminalDTO.ParticipationState which we can trust
            int safeParticipationState;
            if (UserHasTerminalAdministratorPermission())
            {
                // We can trust Fr8 administrator (and he/she can change ParticipationState) so just get it from DTO
                safeParticipationState = terminal.ParticipationState;
            }
            else
            {
                if (terminal.InternalId == Guid.Empty)
                {
                    // For new terminals the value is 0 (Unapproved)
                    safeParticipationState = ParticipationState.Unapproved;
                }
                else
                {
                    // We cannot trust user so get the value from the DB
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        if(_securityService.AuthorizeActivity(PermissionType.UseTerminal, terminal.InternalId, nameof(TerminalDO)))
                        {
                            var terminalTempDo = uow.TerminalRepository.GetByKey(terminal.InternalId);
                            if (terminalTempDo == null)
                            {
                                throw new Fr8NotFoundException(nameof(terminal.InternalId), $"Terminal with the id {terminal.InternalId} is not found.");
                            }
                            safeParticipationState = terminalTempDo.ParticipationState;
                        }
                        else
                        {
                            throw new HttpException(403, $"You are not authorized to use Terminal {terminal.Name}!");
                        }
                    }
                }
            }
            terminal.ParticipationState = safeParticipationState;

            // Validate data
            if (terminal.ParticipationState == ParticipationState.Approved)
            {
                if (string.IsNullOrWhiteSpace(terminal.ProdUrl))
                {
                    throw new Fr8ArgumentNullException(nameof(terminal.ProdUrl), "Production endpoint must be specified for the terminals in the Approved state.");
                }
                curEndpoint = NormalizeUrl(terminal.ProdUrl);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(terminal.DevUrl))
                {
                    throw new Fr8ArgumentNullException(nameof(terminal.DevUrl), "Development endpoint must be specified for the terminals in the Unapproved, Blocked or Deleted state.");
                }
                curEndpoint = NormalizeUrl(terminal.DevUrl);
            }

#if DEBUG
            // Use local URL for Fr8 own terminals when in the local environment or during FBB tests.
            if (terminal.IsFr8OwnTerminal)
            {
                curEndpoint = NormalizeUrl(terminal.DevUrl);
            }
#endif

            if (curEndpoint.Contains("/discover", StringComparison.OrdinalIgnoreCase))
            {
                throw new Fr8ArgumentException(nameof(terminal.Endpoint), "Invalid terminal URL", "Terminal URL should not contain 'discover'. Please correct the URL and try again.");
            }

            if (!UserHasTerminalAdministratorPermission())
            {
                // Developer cannot add terminals with the "localhost" endpoint,
                // or set it while editing, or assign a terminal the Fr8OwnTerminal flag,
                // or edit the terminal with such a flag.
                // User must be an administrator to add or edit a Fr8 own terminal.
                if ((new Uri(curEndpoint).Host == "localhost"))
                {
                    throw new Fr8InsifficientPermissionsException("Insufficient permissions to add a 'localhost' endpoint.",
                        "Terminal URL cannot contain the string 'localhost'. Please correct your terminal URL and try again.");
                }

                if (terminal.IsFr8OwnTerminal)
                {
                    throw new Fr8InsifficientPermissionsException("Insufficient permissions to manage a Fr8Own terminal.",
                        "Terminal URL cannot contain the string 'localhost'. Please correct your terminal URL and try again.");
                }
            }

            // Validating discovery response 
            if (terminal.ParticipationState == ParticipationState.Approved ||
                terminal.ParticipationState == ParticipationState.Unapproved)
            {
                if (!curEndpoint.Contains("://localhost"))
                {
                    string errorMessage = "Terminal at the specified URL did not return a valid response to the discovery request.";
                    try
                    {
                        var terminalRegistrationInfo = await SendDiscoveryRequest(curEndpoint, null);
                        if (terminalRegistrationInfo == null)
                        {
                            Logger.Info($"Terminal at '{curEndpoint}' returned an invalid response.");
                            throw new Fr8ArgumentException(nameof(terminal.Endpoint), errorMessage, errorMessage);
                        }
                        if (string.IsNullOrEmpty(terminalRegistrationInfo.Definition.Name))
                        {
                            string validationErrorMessage = $"Validation of terminal at '{curEndpoint}' failed: Terminal Name is empty.";
                            Logger.Info(validationErrorMessage);
                            throw new Fr8ArgumentException(nameof(terminal.Endpoint), validationErrorMessage, validationErrorMessage);
                        }
                    }
                    catch (TaskCanceledException ex)
                    {
                        string errorMessase = $"Terminal at '{curEndpoint}' did not respond to a /discovery request within 10 sec.";
                        Logger.Info(errorMessase);
                        throw new Fr8ArgumentException(nameof(terminal.Endpoint), errorMessase, "The terminal did not respond to a discovery request within 10 seconds.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Info($"Terminal at '{curEndpoint}' returned an invalid response.");
                        throw new Fr8ArgumentException(nameof(terminal.Endpoint), ex.ToString(), errorMessage);
                    }
                }
            }


            var terminalDo = new TerminalDO();
            terminalDo.Endpoint = terminal.Endpoint = curEndpoint;

            //Check whether we save an existing terminal or register a new one
            if (terminal.InternalId == Guid.Empty)
            {
                Logger.Info($"Registration of terminal at '{curEndpoint}' is requested. ");

                // New terminal
                if (IsExistingTerminal(curEndpoint))
                {
                    Logger.Error($"Terminal with endpoint '{curEndpoint}' was already registered");
                    throw new Fr8ConflictException(nameof(TerminalDO), nameof(TerminalDO.Endpoint), curEndpoint);
                }

                terminalDo.TerminalStatus = TerminalStatus.Undiscovered;

                // The 'Endpoint' property contains the currently active endpoint which may be changed 
                // by deployment scripts or by promoting the terminal from Dev to Production 
                // while ProdUrl/DevUrl contains  whatever user or administrator have supplied.   

                // Set properties which can be safely set by any user             
                terminalDo.DevUrl = terminal.DevUrl;

                if (UserHasTerminalAdministratorPermission())
                {
                    // Set properties which can only be set by Administrator
                    terminalDo.ParticipationState = terminal.ParticipationState;
                    terminalDo.IsFr8OwnTerminal = terminal.IsFr8OwnTerminal;
                    terminalDo.ProdUrl = terminal.ProdUrl;
                }
                else
                {
                    // If a Developer adds a terminal, it has to be approved by Fr8 Administrator
                    terminalDo.ParticipationState = ParticipationState.Unapproved;
                }

                terminalDo.UserId = Thread.CurrentPrincipal.Identity.GetUserId();
            }
            else
            {
                // An existing terminal
                terminalDo.Id = terminal.InternalId;
                terminalDo.DevUrl = terminal.DevUrl;

                //Administrator can update production URL and ParticipationState
                if (UserHasTerminalAdministratorPermission())
                {
                    terminalDo.ProdUrl = terminal.ProdUrl;
                    terminalDo.ParticipationState = terminal.ParticipationState;
                    terminalDo.IsFr8OwnTerminal = terminal.IsFr8OwnTerminal;
                }
            }

            if (terminal.InternalId == Guid.Empty)
            {
                Logger.Info($"Proceeding to registering a new terminal: " + JsonConvert.SerializeObject(terminalDo));
            }
            else
            {
                Logger.Info($"Proceeding to update of an existing terminal: " + JsonConvert.SerializeObject(terminalDo));
            }

            terminalDo = _terminal.RegisterOrUpdate(terminalDo, true);

            Logger.Info($"Terminal at '{terminalDo.Endpoint}' (id: {terminalDo.Id}) was successfully saved.");

            if (terminalDo.ParticipationState == ParticipationState.Approved ||
                terminalDo.ParticipationState == ParticipationState.Unapproved)
            {
                bool discoveryResult = (await DiscoverInternal(terminalDo, true)).IsSucceed;
                if (!discoveryResult)
                {
                    Logger.Info($"The terminal at {curEndpoint} has been registered but an error has occurred while carrying out discovery.");
                }
            }
        }

        private bool IsExistingTerminal(string endpoint)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                string authority = new Uri(endpoint).Authority;

                Func<TerminalDO, bool> predicate = x =>
                    (!string.IsNullOrEmpty(x.DevUrl) && string.Equals((new Uri(x.DevUrl).Authority), authority, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.ProdUrl) && string.Equals((new Uri(x.ProdUrl).Authority), authority, StringComparison.OrdinalIgnoreCase));

                return uow.TerminalRepository.GetAll().Any(predicate);
            }
        }

        private bool UserHasTerminalAdministratorPermission()
        {
            return _securityService.UserHasPermission(PermissionType.EditAllObjects, "TerminalDO");
        }

        public async Task DiscoverAll()
        {
            var discoverTerminalsTasks = _terminal.GetAll().Select(x => DiscoverInternal(x, false)).ToArray();

            await Task.WhenAll(discoverTerminalsTasks);
        }

        private async Task<StandardFr8TerminalCM> SendDiscoveryRequest(string terminalUrl, Dictionary<string, string> headers = null)
        {
            return await _restfulServiceClient.GetAsync<StandardFr8TerminalCM>(new Uri(terminalUrl + "/discover", UriKind.Absolute), null, headers);
        }

        public async Task<DiscoveryResult> Discover(TerminalDTO terminal, bool isUserInitiated)
        {
            TerminalDO existentTerminal = null;

            if (!string.IsNullOrWhiteSpace(terminal.Name) && !string.IsNullOrWhiteSpace(terminal.Version))
            {
                // TODO: @alexavrutin: This comparison is not going to work in the long term. 
                // Terminal names are not guaranateed to be unique. Consider changing to Endpoint.
                existentTerminal = _terminal.GetAll().FirstOrDefault(x => !string.Equals(x.Name, terminal.Name, StringComparison.OrdinalIgnoreCase) && x.Version == terminal.Version);

                if (existentTerminal != null)
                {
                    Logger.Info($"Discovering of terminal Name: {terminal.Name}, Version: {terminal.Version} was requested...");
                }
            }

            if (existentTerminal == null)
            {
                if (string.IsNullOrWhiteSpace(terminal.Endpoint))
                {
                    var message = "No endpoint was specified for discovery request";
                    Logger.Warn(message);
                    return DiscoveryResult.Error(message);
                }

                Logger.Info($"Discovering of  terminal at '{terminal.Endpoint}' was requested...");

                existentTerminal = _terminal.GetAll().FirstOrDefault(x => string.Equals(NormalizeUrl(x.Endpoint), NormalizeUrl(terminal.Endpoint), StringComparison.OrdinalIgnoreCase));
            }

            if (existentTerminal == null)
            {
                var message = $"Discovery for terminal '{terminal.Endpoint}' failed: the provided Endpoint is not found in the Endpoint field of any of the existing Terminal entries.";
                Logger.Info(message);
                return DiscoveryResult.Error(message);
            }

            return await DiscoverInternal(existentTerminal, isUserInitiated);
        }

        private static string NormalizeUrl(string terminalUrl)
        {
            if (!terminalUrl.Contains("http:") & !terminalUrl.Contains("https:"))
            {
                terminalUrl = "http://" + terminalUrl.TrimStart('\\', '/');
            }

            return terminalUrl.TrimEnd('/', '\\');
        }

        //private async Task<StandardFr8TerminalCM> SendDiscoveryRequest(string terminalUrl, Dictionary<string, string> headers = null)
        //{
        //    // Use a custom HttpClient to query the terminal with a low timeout (10 sec)
        //    var innerClient = new HttpClient();
        //    innerClient.Timeout = new TimeSpan(0, 0, 10);
        //    try
        //    {
        //        var response = await innerClient.GetAsync(new Uri(terminalUrl + "/discover", UriKind.Absolute));
        //        response.EnsureSuccessStatusCode();
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        return JsonConvert.DeserializeObject<StandardFr8TerminalCM>(responseContent);
        //    }
        //    catch (Exception) // Expect an exception if the request failed
        //    {
        //        throw;
        //    }
        //}

        private async Task<DiscoveryResult> DiscoverInternal(TerminalDO terminalDo, bool isUserInitiated)
        {
            var terminalUrl = terminalDo.Endpoint;

            Logger.Info($"Starting discovering terminal at '{terminalUrl}'. Reporting about self as the Hub at '{_serverUrl}'.");


            string secret = null;

            if (terminalDo.ParticipationState == ParticipationState.Blocked)
            {
                var message = $"Discovery for terminal '{terminalUrl}' will not happen because the terminal is blocked.";

                Logger.Info(message);
                return DiscoveryResult.Error(message);
            }

            if (terminalDo.ParticipationState == ParticipationState.Deleted)
            {
                var message = $"Discovery for terminal '{terminalUrl}' will not happen because the terminal is deleted.";
                Logger.Info(message);
                return DiscoveryResult.Error(message);
            }

            if (!string.IsNullOrWhiteSpace(terminalDo?.Secret))
            {
                secret = terminalDo.Secret;
            }

            if (secret == null)
            {
                Logger.Info($"Generating new secret for terminal at '{terminalUrl}'");
                secret = Guid.NewGuid().ToString("N");
            }

            var headers = new Dictionary<string, string>
            {
                {"Fr8HubCallbackSecret", secret},
                {"Fr8HubCallBackUrl", _serverUrl}
            };

            StandardFr8TerminalCM terminalRegistrationInfo = null;

            try
            {
                terminalRegistrationInfo = await SendDiscoveryRequest(terminalUrl, headers);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to call terminal discovery endpoint", ex);

                _eventReporter.ActivityTemplateTerminalRegistrationError($"Failed terminal service: {terminalUrl}. Error Message: {ex.Message} ", ex.GetType().Name);
                terminalRegistrationInfo = null;
            }

            if (terminalRegistrationInfo == null || terminalRegistrationInfo.Definition == null || terminalRegistrationInfo.Activities == null)
            {
                // Discovery failed
                var message = $"Terminal at '{terminalUrl}'  didn't return a valid response to the discovery request.";

                Logger.Warn(message);
                // Set terminal status inactive 
                terminalDo.OperationalState = OperationalState.Inactive;

                try
                {
                    _terminal.RegisterOrUpdate(terminalDo, isUserInitiated);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to update information about the terminal.", ex);
                }

                return DiscoveryResult.Error(message);
            }

            terminalDo.Secret = secret;
            terminalDo.OperationalState = OperationalState.Active;
            terminalDo.AuthenticationType = terminalRegistrationInfo.Definition.AuthenticationType;
            terminalDo.Description = terminalRegistrationInfo.Definition.Description;
            terminalDo.Label = terminalRegistrationInfo.Definition.Label;
            terminalDo.Name = terminalRegistrationInfo.Definition.Name;
            terminalDo.Version = terminalRegistrationInfo.Definition.Version;
            terminalDo.TerminalStatus = terminalRegistrationInfo.Definition.TerminalStatus;

            if (string.IsNullOrWhiteSpace(terminalDo.Label))
            {
                terminalDo.Label = terminalDo.Name;
            }

            try
            {
                terminalDo = _terminal.RegisterOrUpdate(terminalDo, isUserInitiated);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to update information about the terminal.", ex);
                return DiscoveryResult.Error($"Internal error updating the information about the terminal at '{terminalUrl}'");
            }

            var activityTemplates = terminalRegistrationInfo.Activities.Select(Mapper.Map<ActivityTemplateDO>).ToList();
            var result = new DiscoveryResult(true, null);

            foreach (var curItem in activityTemplates)
            {
                Logger.Info($"Registering activity '{curItem.Name}' from terminal at '{terminalUrl}'");

                try
                {
                    curItem.Terminal = terminalDo;
                    curItem.TerminalId = terminalDo.Id;

                    _activityTemplateService.RegisterOrUpdate(curItem);
                    result.SucceededTemplates.Add(curItem);
                }
                catch (Exception ex)
                {
                    _eventReporter.ActivityTemplateTerminalRegistrationError($"Failed to register activity {curItem.Name} of version {curItem.Version} for terminal {terminalDo.Name}. Error Message: {ex.Message}", ex.GetType().Name);

                    Logger.Warn($"Failed to register activity {curItem.Name} of version {curItem.Version} for terminal {terminalDo.Name}", ex);
                    result.FailedTemplates.Add(curItem);
                }
            }

            _activityTemplateService.RemoveInactiveActivities(terminalDo, activityTemplates);

            Logger.Info($"Successfully discovered terminal at '{terminalUrl}'.");

            return result;
        }
    }
}
