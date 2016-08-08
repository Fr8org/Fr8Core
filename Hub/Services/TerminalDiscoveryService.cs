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
using Data.States;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Linq.Expressions;
using Hub.Exceptions;
using StructureMap;

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
        private readonly HashSet<string> _knownTerminals = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
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

            var terminalUrls = ListTerminalEndpoints();

            foreach (var url in terminalUrls)
            {
                string terminalAuthority = url;

                if (url.Contains("http:") || url.Contains("https:"))
                {
                    try
                    {
                        terminalAuthority = new Uri(url).Authority;
                    }
                    catch
                    {
                        continue;
                    }
                }

                _knownTerminals.Add(terminalAuthority);
            }
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
                if (terminal.InternalId == 0)
                {
                    // For new terminals the value is 0 (Unapproved)
                    safeParticipationState = ParticipationState.Unapproved;
                }
                else
                {
                    // We cannot trust user so get the value from the DB
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var terminalTempDo = uow.TerminalRepository.GetByKey(terminal.InternalId); //TODO: check user permissions here!!
                        if (terminalTempDo == null)
                        {
                            throw new ArgumentOutOfRangeException($"Terminal with the id {terminal.InternalId} is not found.", nameof(terminal.InternalId));
                        }
                        safeParticipationState = terminalTempDo.ParticipationState;
                    }
                }
            }
            terminal.ParticipationState = safeParticipationState;

            // Validate data
            if (terminal.ParticipationState == ParticipationState.Approved)
            {
                if (string.IsNullOrWhiteSpace(terminal.ProdUrl))
                {
                    throw new ArgumentNullException("Production endpoint must be specified for the terminals in the Approved state.", nameof(terminal.ProdUrl));
                }
                curEndpoint = NormalizeUrl(terminal.ProdUrl);
            }
            else if (terminal.ParticipationState == ParticipationState.Unapproved)
            {
                if (string.IsNullOrWhiteSpace(terminal.DevUrl))
                {
                    throw new ArgumentNullException("Development endpoint must be specified for the terminals in the Unapproved state.", nameof(terminal.DevUrl));
                }
                curEndpoint = NormalizeUrl(terminal.DevUrl);
            }
            else
            {
                curEndpoint = string.Empty;
            }
#if DEBUG
            // Use local URL for Fr8 own terminals when in the local environment or during FBB tests.
            if (terminal.IsFr8OwnTerminal)
            {
                curEndpoint = NormalizeUrl(terminal.DevUrl);
            }
#endif

            if (!UserHasTerminalAdministratorPermission())
            {
                // Developer cannot add terminals with the "localhost" endpoint,
                // or set it while editing, or assign a terminal the Fr8OwnTerminal flag,
                // or edit the terminal with such a flag.
                // User must be an administrator to add or edit a Fr8 own terminal.
                if ((new Uri(curEndpoint).Host == "localhost"))
                {
                    throw new InvalidOperationException("Insufficient permissions to add a 'localhost' endpoint.");
                }

                if (terminal.IsFr8OwnTerminal)
                {
                    throw new InvalidOperationException("Insufficient permissions to manage a Fr8Own terminal.");
                }
            }

            Logger.Info($"Registration of terminal at '{curEndpoint}' is requested.");

            var terminalDo = new TerminalDO();
            terminalDo.Endpoint = terminal.Endpoint = curEndpoint;

            //Check whether we save an existing terminal or register a new one
            if (terminal.InternalId == 0)
            {
                // New terminal
                if (IsExistingTerminal(curEndpoint))
                {
                    Logger.Error($"Terminal with endpoint '{curEndpoint}' was already registered");
                    throw new ConflictException(nameof(TerminalDO), nameof(TerminalDO.Endpoint), curEndpoint);
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
                    terminalDo.ProdUrl = terminal.ProdUrl;
                }
                else
                {
                    // If a Developer adds a terminal, it has to be approved by Fr8 Administrator
                    terminalDo.ParticipationState = ParticipationState.Unapproved;
                }

                terminalDo.UserId = Thread.CurrentPrincipal.Identity.GetUserId();
                Logger.Info($"Terminal at '{curEndpoint}' was successfully registered.");

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
                }
            }

            terminalDo = _terminal.RegisterOrUpdate(terminalDo, true);
            Logger.Info($"Terminal at '{curEndpoint}' was successfully registered.");

            if (!await DiscoverInternal(curEndpoint, true))
            {
                Logger.Info($"The terminal at {curEndpoint} has been registered but an error has occurred while carrying out discovery.");
            }
        }

        private bool IsExistingTerminal(string endpoint)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                string authority = new Uri(endpoint).Authority;

                Func<TerminalDO, bool> predicate = x =>
                    string.Equals((new Uri(x.DevUrl).Authority), authority, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals((new Uri(x.ProdUrl).Authority), authority, StringComparison.OrdinalIgnoreCase);

                return uow.TerminalRepository.GetAll().Any(predicate);
            }
        }

        private bool UserHasTerminalAdministratorPermission()
        {
            return _securityService.UserHasPermission(PermissionType.EditAllObjects, "TerminalDO");
        }

        public async Task DiscoverAll()
        {
            var terminalUrls = ListTerminalEndpoints();
            var discoverTerminalsTasks = terminalUrls.Select(x => DiscoverInternal(NormalizeUrl(x), false)).ToArray();

            await Task.WhenAll(discoverTerminalsTasks);
        }

        public async Task<bool> Discover(string terminalUrl, bool isUserInitiated)
        {
            Logger.Info($"Discovering of  terminal at '{terminalUrl}' was requested...");

            // validate terminal url
            var uri = new Uri(terminalUrl);

            lock (_knownTerminals)
            {
                if (!_knownTerminals.Contains(uri.Authority))
                {
                    Logger.Info($"Terminal at at '{terminalUrl}' was not registered within the Hub. Discovery request declied.");
                    return false;
                }
            }

            return await DiscoverInternal(NormalizeUrl(terminalUrl), isUserInitiated);
        }

        private static string NormalizeUrl(string terminalUrl)
        {
            if (!terminalUrl.Contains("http:") & !terminalUrl.Contains("https:"))
            {
                terminalUrl = "http://" + terminalUrl.TrimStart('\\', '/');
            }

            return terminalUrl;
        }

        private async Task<bool> DiscoverInternal(string terminalUrl, bool isUserInitiated)
        {
            Logger.Info($"Starting discovering terminal at '{terminalUrl}'. Reporting about self as the Hub at '{_serverUrl}'.");

            try
            {
                string secret = null;
                var terminalDo = _terminal.GetAll().FirstOrDefault(x => string.Equals(NormalizeUrl(x.Endpoint), NormalizeUrl(terminalUrl), StringComparison.OrdinalIgnoreCase));

                if (terminalDo == null)
                {
                    Logger.Info($"Discovery for terminal '{terminalUrl}' failed: the provided Endpoint is not found in the Endpoint field of any of the existing Terminal entries.");
                    return false;
                }

                if (terminalDo.ParticipationState == ParticipationState.Blocked)
                {
                    Logger.Info($"Discovery for terminal '{terminalUrl}' will not happen because the terminal is blocked.");
                    return false;
                }

                if (terminalDo.ParticipationState == ParticipationState.Deleted)
                {
                    Logger.Info($"Discovery for terminal '{terminalUrl}' will not happen because the terminal is deleted.");
                    return false;
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
                    { "Fr8HubCallBackUrl", _serverUrl}
                };

                var terminalRegistrationInfo = await _restfulServiceClient.GetAsync<StandardFr8TerminalCM>(new Uri(terminalUrl + "/discover", UriKind.Absolute), null, headers);

                if (terminalRegistrationInfo == null)
                {
                    Logger.Error($"Terminal at '{terminalUrl}'  didn't return a valid response to the discovery request.");

                    // Set terminal status inactive 

                    return false;
                }

                var activityTemplates = terminalRegistrationInfo.Activities.Select(Mapper.Map<ActivityTemplateDO>).ToList();

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

                terminalDo = _terminal.RegisterOrUpdate(terminalDo, isUserInitiated);

                foreach (var curItem in activityTemplates)
                {
                    Logger.Info($"Registering activity '{curItem.Name}' from terminal at '{terminalUrl}'");
                    try
                    {
                        curItem.Terminal = terminalDo;
                        curItem.TerminalId = terminalDo.Id;

                        _activityTemplateService.RegisterOrUpdate(curItem);
                    }
                    catch (Exception ex)
                    {
                        _eventReporter.ActivityTemplateTerminalRegistrationError($"Failed to register {curItem.Terminal.Name} terminal. Error Message: {ex.Message}", ex.GetType().Name);
                    }
                }

                _activityTemplateService.RemoveInactiveActivities(activityTemplates);
            }
            catch (Exception ex)
            {
                _eventReporter.ActivityTemplateTerminalRegistrationError($"Failed terminal service: {terminalUrl}. Error Message: {ex.Message} ", ex.GetType().Name);
                return false;
            }

            lock (_knownTerminals)
            {
                var uri = new Uri(terminalUrl);
                _knownTerminals.Add(uri.Authority);
            }

            Logger.Info($"Successfully discovered terminal at '{terminalUrl}'.");

            return true;
        }

        private string[] ListTerminalEndpoints()
        {
            var terminalUrls = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            using (var uow = _unitOfWorkFactory.Create())
            {
                foreach (var terminalRegistration in uow.TerminalRepository.GetAll())
                {
                    terminalUrls.Add(terminalRegistration.Endpoint);
                }
            }

            return terminalUrls.ToArray();
        }
    }
}
