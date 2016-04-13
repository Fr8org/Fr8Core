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

namespace terminalDocuSign.Actions
{
    public class DocuSign_Polling_v1 : BaseDocuSignActivity
    {
        public DocuSign_Polling_v1()
        {

        }

        protected override string ActivityUserFriendlyName => "DocuSign Polling";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            
            //EnvelopesApi api = new EnvelopesApi();
            //api.ListStatusChanges()       

            return Success(await GetPayload(curActivityDO, containerId));
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