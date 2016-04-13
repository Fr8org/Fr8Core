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

namespace terminalDocuSign.Actions
{
    public class Notify_DocuSign_Events_v1 : BaseDocuSignActivity
    {
        protected override string ActivityUserFriendlyName => "Notify DocuSign Events";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
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
                storage.Add(PackControls(new StandardConfigurationControlsCM() { Controls = new List<ControlDefinitionDTO>()
                { new TextBlock { Value = "This activity doesn't require any configuration" } } }));
            }
            return curActivityDO;
        }
    }
}