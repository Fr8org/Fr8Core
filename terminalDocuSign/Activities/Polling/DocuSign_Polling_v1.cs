using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;
using Hub.Managers;
using Data.States;
using terminalDocuSign.Services.New_Api;
using StructureMap;
using TerminalBase.Infrastructure;
using terminalDocuSign.Actions;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

namespace terminalDocuSign.Actions
{
    public class DocuSign_Polling_v1 : BaseDocuSignActivity
    {
        private IDocuSignManager _docusignManager;

        public DocuSign_Polling_v1()
        {
            _docusignManager = ObjectFactory.GetInstance<IDocuSignManager>();
        }

        protected override string ActivityUserFriendlyName => "DocuSign Polling";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var config = _docusignManager.SetUp(authTokenDO);
            EnvelopesApi api = new EnvelopesApi((Configuration)config.Configuration);
            var changed_envelopes = api.ListStatusChanges(config.AccountId, new EnvelopesApi.ListStatusChangesOptions() { fromDate = DateTime.UtcNow.AddMinutes(-16).ToString("o") });

            var payload = await GetPayload(curActivityDO, containerId);

            using (var updatable_storage = CrateManager.GetUpdatableStorage(payload))
            {
                foreach (var envelope in changed_envelopes.Envelopes)
                {
                    var envelopeCrate = Data.Crates.Crate.FromContent("ChangedEnvelope", new DocuSignEnvelopeCM_v2()
                    {
                        EnvelopeId = envelope.EnvelopeId,
                        Status = envelope.Status,
                        StatusChangedDateTime = DateTime.Parse(envelope.StatusChangedDateTime)
                    }, AvailabilityType.RunTime);
                    updatable_storage.Add(envelopeCrate);
                }
            }

            return Success(payload);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return CrateManager.IsStorageEmpty(curActivityDO) ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        protected async override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var storage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                storage.Add(PackControls(new StandardConfigurationControlsCM()
                {
                    Controls = new List<ControlDefinitionDTO>()
                { new TextBlock { Value = "This activity doesn't require any configuration" } }
                }));
            }
            return curActivityDO;
        }
    }
}