
using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using System.Collections.Generic;

namespace terminalPapertrial.Actions
{
    /// <summary>
    /// Write To Log action which writes Log Messages to Papertrial at run time
    /// </summary>
    public class Write_To_Log_v1 : BaseTerminalAction
    {
        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            var targetUrlTextBlock = new TextBoxControlDefinitionDTO
            {
                Name = "TargetUrlTextBox",
                Label = "Target Papertrial URL (As URL:PORT format)",
                Value = CloudConfigurationManager.GetSetting("PapertrialDefaultLogUrl"),
                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
            };

            var curControlsCrate = PackControlsCrate(targetUrlTextBlock);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = new CrateStorage(curControlsCrate);
            }

            return await Task.FromResult(curActionDO);
        }

        public Task<object> Activate(ActionDO curActionDO)
        {
            //Responsibility is not yet defined.

            return Task.FromResult((object) true);
        }

        public object Deactivate(ActionDO curDataPackage)
        {
            //Responsibility is not yet defined.

            return true;
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curProcessPayload = await GetProcessPayload(containerId);

            //var curLogMessages = Crate.GetStorage(curProcessPayload).CrateContentsOfType<StandardLoggingCM>().First();

            //check the current log messages and write the log to papertrial

            return curProcessPayload;
        }
    }
}