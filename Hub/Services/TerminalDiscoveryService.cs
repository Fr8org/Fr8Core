using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Hub.Managers;

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

            var terminalUrls = FileUtils.LoadFileHostList();
            

            foreach (var url in terminalUrls)
            {
                string terminalAuthority = url;

                if (url.Contains("http:"))
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

        public async Task Discover()
        {
            var terminalUrls = FileUtils.LoadFileHostList();
            var discoverTerminalsTasts = terminalUrls.Select(Discover).ToArray();

            await Task.WhenAll(discoverTerminalsTasts);
        }

        public async Task<bool> Discover(string terminalUrl)
        {
            // validate terminal url
            var uri = new Uri(terminalUrl);

            if (!_knownTerminals.Contains(uri.Authority))
            {
                return false;
            }

            try
            {
                string secret = null;

                using (var uow = _unitOfWorkFactory.GetNewUnitOfWork())
                {
                    var exisitingTerminal = uow.TerminalRepository.GetQuery().FirstOrDefault(x => x.Endpoint == terminalUrl);

                    if (!string.IsNullOrWhiteSpace(exisitingTerminal?.Secret))
                    {
                        secret = exisitingTerminal.Secret;
                    }
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
                var activityTemplates = terminalRegistrationInfo.Activities.Select(Mapper.Map<ActivityTemplateDO>).ToList();

                var terminal = Mapper.Map<TerminalDO>(terminalRegistrationInfo.Definition);

                terminal.Secret = secret;
                
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

            return true;
        }
    }
}
