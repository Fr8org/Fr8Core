using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class StopExecutionIf_v1 : BaseTerminalAction
    {
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActionDO, containerId);
            

            return TerminateHubExecution(curPayloadDTO);
        }


        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
            }

            return curActionDO;
        }

        private Crate CreateControlsCrate()
        {
            var duration = new Duration
            {
                Label = "Please enter delay duration",
                Name = "Delay_Duration"
            };


            return PackControlsCrate(duration);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var durationControl = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.Duration && x.Name == "Delay_Duration");

            if (durationControl == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}