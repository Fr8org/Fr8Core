using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class Terminal : ITerminal
    {
        private readonly Dictionary<int, TerminalDO> _terminals = new Dictionary<int, TerminalDO>();
        private bool _isInitialized;
        private string _serverUrl;

        public bool IsATandTCacheDisabled
        {
            get;
            private set;
        }

        public Terminal(IConfigRepository configRepository)
        {
            IsATandTCacheDisabled = string.Equals(CloudConfigurationManager.GetSetting("DisableATandTCache"), "true", StringComparison.InvariantCultureIgnoreCase);

            var serverProtocol = configRepository.Get("ServerProtocol", String.Empty);
            var domainName = configRepository.Get("ServerDomainName", String.Empty);
            var domainPort = configRepository.Get<int?>("ServerPort", null);

            _serverUrl = $"{serverProtocol}{domainName}{(domainPort == null || domainPort.Value == 80 ? String.Empty : (":" + domainPort.Value))}/";
        }

        private void Initialize()
        {
            if (_isInitialized && !IsATandTCacheDisabled)
            {
                return;
            }

            lock (_terminals)
            {
                if (_isInitialized && !IsATandTCacheDisabled)
                {
                    return;
                }

                if (IsATandTCacheDisabled)
                {
                    _terminals.Clear();
                }

                LoadFromDb();

                _isInitialized = true;
            }
        }
        

        private void LoadFromDb()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var existingTerminal in uow.TerminalRepository.GetAll())
                {
                    _terminals[existingTerminal.Id] = Clone(existingTerminal);
                }
            }
        }

        public TerminalDO GetByKey(int terminalId)
        {
            Initialize();

            lock (_terminals)
            {
                TerminalDO terminal;

                if (!_terminals.TryGetValue(terminalId, out terminal))
                {
                    throw new KeyNotFoundException(string.Format("Unable to find terminal with id {0}", terminalId));
                }

                return terminal;
            }
        }

        public TerminalDO GetByNameAndVersion(string name, string version)
        {
            Initialize();

            lock (_terminals)
            {
                TerminalDO terminal =_terminals.FirstOrDefault(t => t.Value.Name == name && t.Value.Version == version).Value;
                if (terminal == null)
                {
                    throw new KeyNotFoundException($"Unable to find terminal with name {name} and version {version}");
                }

                return terminal;
            }
        }

        public TerminalDO RegisterOrUpdate(TerminalDO terminalDo)
        {
            if (terminalDo == null)
            {
                return null;
            }

            if (!IsATandTCacheDisabled)
            {
                Initialize();
            }

            // we are going to change activityTemplateDo. It is not good to corrupt method's input parameters.
            // make a copy
            var clone = new TerminalDO();

            CopyPropertiesHelper.CopyProperties(terminalDo, clone, true);

            terminalDo = clone;

            lock (_terminals)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var existingTerminal = uow.TerminalRepository.FindOne(x => x.Name == terminalDo.Name);

                    if (existingTerminal == null)
                    {
                        terminalDo.Id = 0;
                        uow.TerminalRepository.Add(existingTerminal = terminalDo);
                    }
                    else
                    {
                        // this is for updating terminal
                        CopyPropertiesHelper.CopyProperties(terminalDo, existingTerminal, false, x => x.Name != "Id");
                    }

                    uow.SaveChanges();

                    var terminal = Clone(existingTerminal);
                    _terminals[existingTerminal.Id] = terminal;

                    return terminal;
                }
            }
        }

        public Dictionary<string, string> GetRequestHeaders(TerminalDO terminal)
        {
            Initialize();

            lock (_terminals)
            {
                if (!_terminals.TryGetValue(terminal.Id, out terminal))
                {
                    throw new KeyNotFoundException(string.Format("Unable to find terminal with id {0}", terminal.Id));
                }
            }

            return new Dictionary<string, string>
            {
                {"Fr8HubCallbackSecret", terminal.Secret},
                {"Fr8HubCallBackUrl", _serverUrl}
            };
        }

        private TerminalDO Clone(TerminalDO source)
        {
            var newTerminal = new TerminalDO();

            CopyPropertiesHelper.CopyProperties(source, newTerminal, false);

            return newTerminal;
        }

        public IEnumerable<TerminalDO> GetAll()
        {
            Initialize();

            lock (_terminals)
            {
                return _terminals.Values.ToArray();
            }
        }

        public async Task<IList<ActivityTemplateDO>> GetAvailableActivities(string uri)
        {
            Initialize();

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            var standardFr8TerminalCM = await restClient.GetAsync<StandardFr8TerminalCM>(new Uri(uri, UriKind.Absolute));
            return standardFr8TerminalCM.Activities.Select(Mapper.Map<ActivityTemplateDO>).ToList();
        }

        public async Task<TerminalDO> GetTerminalByPublicIdentifier(string terminalId)
        {
            Initialize();

            lock (_terminals)
            {
                return _terminals.Values.FirstOrDefault(t => t.PublicIdentifier == terminalId);
            }
        }

        public async Task<bool> IsUserSubscribedToTerminal(string terminalId, string userId)
        {
            Initialize();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var subscription = await uow.TerminalSubscriptionRepository.GetQuery()
                    .FirstOrDefaultAsync(s => s.Terminal.PublicIdentifier == terminalId && s.UserDO.Id == userId);
                return subscription != null;
            }

        }

        public async Task<List<DocumentationResponseDTO>> GetSolutionDocumentations(string terminalName)
        {
            var _activity = ObjectFactory.GetInstance<IActivity>();
            var solutionNames = _activity.GetSolutionNameList(terminalName);
            var solutionPages = new List<DocumentationResponseDTO>();
            foreach (var solutionName in solutionNames)
            {
               var solutionPageDTO = await _activity.GetActivityDocumentation<DocumentationResponseDTO>(
                    new ActivityDTO
                    {
                        Documentation = "MainPage",
                        ActivityTemplate = new ActivityTemplateDTO {Name = solutionName }
                    }, true);
                if (solutionPageDTO != null)
                {
                    solutionPages.Add(solutionPageDTO);
                }
            }
            return solutionPages;
        }

        public async Task<TerminalDO> GetByToken(string token)
        {
            Initialize();

            lock (_terminals)
            {
                return _terminals.Values.FirstOrDefault(t => t.Secret == token);
            }
        }

    }
}
