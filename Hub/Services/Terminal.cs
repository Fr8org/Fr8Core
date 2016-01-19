using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using StructureMap;

namespace Hub.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class Terminal : ITerminal
    {
        private readonly Dictionary<int, TerminalDO>  _terminals = new Dictionary<int, TerminalDO>();
        private bool _isInitialized;

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            lock (_terminals)
            {
                if (_isInitialized)
                {
                    return;
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
        
        public void RegisterOrUpdate(TerminalDO terminalDo)
        {
            Initialize();

            lock (_terminals)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var existingTerminal = uow.TerminalRepository.FindOne(x => x.Name == terminalDo.Name);

                    if (existingTerminal == null)
                    {
                        uow.TerminalRepository.Add(existingTerminal = terminalDo);
                        uow.SaveChanges();
                    }
                    else
                    {
                        existingTerminal.AuthenticationType = terminalDo.AuthenticationType;
                        existingTerminal.Description = terminalDo.Description;
                        existingTerminal.Endpoint = terminalDo.Endpoint;
                        existingTerminal.Name = terminalDo.Name;
                        existingTerminal.PublicIdentifier = terminalDo.PublicIdentifier;
                        existingTerminal.Secret = terminalDo.Secret;
                        existingTerminal.SubscriptionRequired = terminalDo.SubscriptionRequired;
                        existingTerminal.TerminalStatus = terminalDo.TerminalStatus;
                        existingTerminal.Version = terminalDo.Version;
                       
                        uow.SaveChanges();
                    }

                    _terminals[existingTerminal.Id] = Clone(existingTerminal); 
                }
            }
        }

        private TerminalDO Clone(TerminalDO source)
        {
            return new TerminalDO
            {
                AuthenticationType = source.AuthenticationType,
                Description = source.Description,
                Endpoint = source.Endpoint,
                Id = source.Id,
                Name = source.Name,
                PublicIdentifier = source.PublicIdentifier,
                Secret = source.Secret,
                SubscriptionRequired = source.SubscriptionRequired,
                TerminalStatus = source.TerminalStatus,
                Version = source.Version
            };
        }

        public IEnumerable<TerminalDO> GetAll()
        {
            Initialize();

            lock (_terminals)
            {
                return _terminals.Values.ToArray();
            }
        }
        
        
        public async Task<IList<ActivityTemplateDO>> GetAvailableActions(string uri)
        {
            Initialize();

            var restClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            var standardFr8TerminalCM = await restClient.GetAsync<StandardFr8TerminalCM>(new Uri(uri, UriKind.Absolute));
            return Mapper.Map<IList<ActivityTemplateDO>>(standardFr8TerminalCM.Actions);
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
                var subscription = await uow.TerminalSubscriptionRepository.GetQuery().FirstOrDefaultAsync(s => s.Terminal.PublicIdentifier == terminalId && s.UserDO.Id == userId);
                return subscription != null;
            }
            
        }
    }
}
