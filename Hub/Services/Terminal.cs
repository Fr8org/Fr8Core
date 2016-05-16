using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using StructureMap;
using Utilities.Configuration.Azure;

namespace Hub.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class Terminal : ITerminal
    {
        private struct TerminalKey
        {
            private readonly string _terminalName;
            private readonly string _terminalVersion;

            public TerminalKey(string terminalName, string terminalVersion)
            {
                _terminalName = terminalName;
                _terminalVersion = terminalVersion;
            }

            private bool Equals(TerminalKey other)
            {
                return string.Equals(_terminalName, other._terminalName) && string.Equals(_terminalVersion, other._terminalVersion);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is TerminalKey && Equals((TerminalKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_terminalName != null ? _terminalName.GetHashCode() : 0) * 397) ^ (_terminalVersion != null ? _terminalVersion.GetHashCode() : 0);
                }
            }
        }

        private class TerminalIdSecretMatch
        {
            public TerminalIdSecretMatch(string terminalId, string terminalSecret)
            {
                TerminalId = terminalId;
                TerminalSecret = terminalSecret;
            }

            public string TerminalId { get; private set; }
            public string TerminalSecret { get; private set; }
        }

        private readonly Dictionary<TerminalKey, TerminalIdSecretMatch> _terminalSecretes = new Dictionary<TerminalKey, TerminalIdSecretMatch>
        {
            {new TerminalKey("terminalSalesforce", "1"), new TerminalIdSecretMatch("d814af88-72b3-444c-9198-8c62292f0be5", "3b685a89-314d-48ce-91c6-7b1cfa29aa21")},
            {new TerminalKey("terminalAzure", "1"), new TerminalIdSecretMatch("e134e36f-9f63-4109-b913-03498d9356b1","8d3d33d9-a260-46e2-b25a-121f2aba2a54")},
            {new TerminalKey("terminalFr8Core", "1"), new TerminalIdSecretMatch( "2db48191-cda3-4922-9cc2-a636e828063f", "9b4a97f3-97ea-42d7-8b02-a208ea47d760")},
            {new TerminalKey("terminalDocuSign", "1"), new TerminalIdSecretMatch("ee29c5bc-b9e7-49c5-90e1-b462c7e320e9", "cc426e06-a42a-4193-9b90-d1122be979a3")},
            {new TerminalKey("terminalSlack", "1"), new TerminalIdSecretMatch("8783174f-7fb7-4947-98af-4f1cdd8b394f", "aa43d09e-a0dd-4433-8b05-4485e57738c6")},
            {new TerminalKey("terminalTwilio", "1"), new TerminalIdSecretMatch("2dd73dda-411d-4e18-8e0a-54bbe1aa015b", "3a772e7d-1368-4173-b081-91a7318910c7")},
            {new TerminalKey("terminalExcel", "1"), new TerminalIdSecretMatch("551acd9b-d91d-4de7-a0ba-8c61be413635", "36392f9d-c3c0-4b6a-a54a-142ba1ce312f")},
            {new TerminalKey("terminalSendGrid", "1"), new TerminalIdSecretMatch("7eab0e81-288c-492b-88e5-c49e9aae38da", "a3a65c3c-6d75-4fd6-bd76-e66047affe09")},
            {new TerminalKey("terminalGoogle", "1"), new TerminalIdSecretMatch("1a170d44-841f-4fa2-aae4-b17ad6c469ec", "ee7a622b-4a12-4dd6-ac09-03caf0da0f25")},
            {new TerminalKey("terminalDropbox", "1"), new TerminalIdSecretMatch("c471e51e-1b2d-4751-b155-4af03ef51c3a", "f6e4a687-fc0b-462a-87de-9cb2729d2bc1")},
            {new TerminalKey("terminalPapertrail", "1"), new TerminalIdSecretMatch("9b21279b-efb4-493a-a02b-fe8694262cc8", "42783cd2-d5e1-4d5a-9ea8-b63922ce2e20")},
            {new TerminalKey("terminalQuickBooks", "1"), new TerminalIdSecretMatch("75ec4967-6113-43b5-bb4c-6b3468696e57", "749f5c59-1bf1-4cb6-9275-eb1d489d9a05")},
            {new TerminalKey("terminalYammer", "1"), new TerminalIdSecretMatch("f2b999be-be3f-42b5-b0d5-611d0606723b", "d14aaa44-22a1-4d2c-b14b-be559c8941b5")},
            {new TerminalKey("terminalAtlassian", "1"), new TerminalIdSecretMatch("d770ec3c-975b-4ca8-910e-a55ac43af383", "f747e49c-63a8-4a1b-8347-dd2e436c3b36")},
            {new TerminalKey("terminalTest", "1"), new TerminalIdSecretMatch("BFA77F6B-969D-4976-B752-930EEB11A4A3", "F4497516-0134-4523-B93C-EB15EE4D0697")},
            {new TerminalKey("terminalBox", "1"), new TerminalIdSecretMatch("1293c430-818d-4326-896b-dd5c512ca1a4","2eb36a7b-6365-4d8f-ad73-a44cdd1d1ebb")},
        };

        private readonly Dictionary<int, TerminalDO> _terminals = new Dictionary<int, TerminalDO>();
        private bool _isInitialized;

        public bool IsATandTCacheDisabled
        {
            get;
            private set;
        }

        public Terminal()
        {
            IsATandTCacheDisabled = string.Equals(CloudConfigurationManager.GetSetting("DisableATandTCache"), "true", StringComparison.InvariantCultureIgnoreCase);
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

        private bool UpdateTerminalSecret(TerminalDO terminal)
        {
            TerminalIdSecretMatch secret;

            if (_terminalSecretes.TryGetValue(new TerminalKey(terminal.Name, terminal.Version), out secret))
            {
                if (terminal.PublicIdentifier != secret.TerminalId || terminal.Secret != secret.TerminalSecret)
                {
                    terminal.PublicIdentifier = secret.TerminalId;
                    terminal.Secret = secret.TerminalSecret;
                    return true;
                }
            }

            return false;
        }

        private void LoadFromDb()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                bool needSaveChanges = false;

                foreach (var existingTerminal in uow.TerminalRepository.GetAll())
                {
                    needSaveChanges |= UpdateTerminalSecret(existingTerminal);
                    _terminals[existingTerminal.Id] = Clone(existingTerminal);
                }

                if (needSaveChanges)
                {
                    uow.SaveChanges();
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

                    UpdateTerminalSecret(existingTerminal);
                    uow.SaveChanges();

                    var terminal = Clone(existingTerminal);
                    _terminals[existingTerminal.Id] = terminal;

                    return terminal;
                }
            }
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

        public async Task<List<SolutionPageDTO>> GetSolutionDocumentations(string terminalName)
        {
            var _activity = ObjectFactory.GetInstance<IActivity>();
            var solutionNames = _activity.GetSolutionNameList(terminalName);
            var solutionPages = new List<SolutionPageDTO>();
            foreach (var solutionName in solutionNames)
            {
               var solutionPageDTO = await _activity.GetActivityDocumentation<SolutionPageDTO>(
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
       
    }
}
