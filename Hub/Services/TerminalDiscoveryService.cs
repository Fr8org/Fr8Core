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
using Microsoft.AspNet.Identity;

namespace Hub.Services
{
    public class TerminalDiscoveryService : ITerminalDiscoveryService
    {
        private readonly IActivityTemplate _activityTemplateService;
        private readonly ITerminal _terminal;
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly EventReporter _eventReporter;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly string _serverUrl;
        private readonly HashSet<string> _knownTerminals = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public TerminalDiscoveryService(IActivityTemplate activityTemplateService, ITerminal terminal, IRestfulServiceClient restfulServiceClient, EventReporter eventReporter, IUnitOfWorkFactory unitOfWorkFactory, IConfigRepository configRepository)
        {
            _activityTemplateService = activityTemplateService;
            _terminal = terminal;
            _restfulServiceClient = restfulServiceClient;
            _eventReporter = eventReporter;
            _unitOfWorkFactory = unitOfWorkFactory;

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

        public async Task RegisterTerminal(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Invalid url", nameof(endpoint));
            }

            endpoint = ExtractTerminalAuthority(endpoint);
            
            using (var uow = _unitOfWorkFactory.Create())
            {
                var terminalRegistration = new TerminalRegistrationDO();

                if (uow.TerminalRegistrationRepository.GetAll().FirstOrDefault(x => string.Equals(ExtractTerminalAuthority(x.Endpoint), endpoint, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    throw new Exception($"Terminal with endpoint '{endpoint}' was already registered");
                }
                 
                terminalRegistration.UserId = Thread.CurrentPrincipal.Identity.GetUserId();
                terminalRegistration.Endpoint = endpoint.ToLower();

                var normaizedEndpoint = NormalizeTerminalEndpoint(endpoint);

                if (!await DiscoverInternal(normaizedEndpoint))
                {
                    throw new Exception($"Unable to discover terminal at '{normaizedEndpoint}'");
                }
                
                uow.TerminalRegistrationRepository.Add(terminalRegistration);
                uow.SaveChanges();
            }
        }

        public async Task Discover()
        {
            var terminalUrls = ListTerminalEndpoints();
            var discoverTerminalsTasts = terminalUrls.Select(x=> DiscoverInternal(NormalizeTerminalEndpoint(x))).ToArray();

            await Task.WhenAll(discoverTerminalsTasts);
        }
        
        public async Task<bool> Discover(string terminalUrl)
        {
            // validate terminal url
            var uri = new Uri(terminalUrl);

            lock (_knownTerminals)
            {
                if (!_knownTerminals.Contains(uri.Authority))
                {
                    return false;
                }
            }

            return await DiscoverInternal(NormalizeTerminalEndpoint(terminalUrl));
        }

        private static string ExtractTerminalAuthority(string terminalUrl)
        {
            string terminalAuthority = terminalUrl;

            if (!terminalUrl.Contains("http:") & !terminalUrl.Contains("https:"))
            {
                terminalAuthority = "http://" + terminalUrl.TrimStart('\\', '/');
            }

            var discoverOp = terminalAuthority.IndexOf("/discover", StringComparison.OrdinalIgnoreCase);

            if (discoverOp > 0)
            {
                terminalAuthority = terminalAuthority.Substring(0, discoverOp);
            }

            return terminalAuthority.TrimEnd('\\', '/');
        }

        private async Task<bool> DiscoverInternal(string terminalUrl)
        {
            try
            {
                string secret = null;
                var terminalAuthority = ExtractTerminalAuthority(terminalUrl);
                var exisitingTerminal = _terminal.GetAll().FirstOrDefault(x => string.Equals(ExtractTerminalAuthority(x.Endpoint), terminalAuthority, StringComparison.OrdinalIgnoreCase)); 

                if (!string.IsNullOrWhiteSpace(exisitingTerminal?.Secret))
                {
                    secret = exisitingTerminal.Secret;
                }

                if (secret == null)
                {
                    secret = Guid.NewGuid().ToString("N");
                }

                var headers = new Dictionary<string, string>
                {
                    {"Fr8HubCallbackSecret", secret},
                    { "Fr8HubCallBackUrl", _serverUrl}
                };

                var terminalRegistrationInfo = await _restfulServiceClient.GetAsync<StandardFr8TerminalCM>(new Uri(terminalUrl, UriKind.Absolute), null, headers);

                if (terminalRegistrationInfo == null)
                {
                    throw new Exception("Terminal didn't return meaningfull reply for discovery request.");
                }

                var activityTemplates = terminalRegistrationInfo.Activities.Select(Mapper.Map<ActivityTemplateDO>).ToList();

                var terminal = Mapper.Map<TerminalDO>(terminalRegistrationInfo.Definition);

                terminal.Secret = secret;

                if (string.IsNullOrWhiteSpace(terminal.Label))
                {
                    terminal.Label = terminal.Name;
                }

                terminal = _terminal.RegisterOrUpdate(terminal);

                foreach (var curItem in activityTemplates)
                {
                    try
                    {
                        curItem.Terminal = terminal;
                        curItem.TerminalId = terminal.Id;

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

            return true;
        }

        private string NormalizeTerminalEndpoint(string endpoint)
        {
            var authority = ExtractTerminalAuthority(endpoint);

            return authority.ToLower() + "/discover";
        }

        private string[] ListTerminalEndpoints()
        {
            var terminalUrls = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            using (var uow = _unitOfWorkFactory.Create())
            {
                foreach (var terminalRegistration in uow.TerminalRegistrationRepository.GetAll())
                {
                    terminalUrls.Add(terminalRegistration.Endpoint);
                }
            }

            return terminalUrls.ToArray();
        }
    }
}
