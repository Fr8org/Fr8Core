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
            var info = new TextBlock
            {
                Label = "This action stops container execution if it can't find shown fields in a StandardPayloadDataCM",
                Name = "InfoText"
            };

            var fieldFilterPane = new FieldList
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Required = true
            };


            return PackControlsCrate(info, fieldFilterPane);
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

            var upstreamChooser = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.UpstreamDataChooser && x.Name == "Upstream_data_chooser");

            if (upstreamChooser == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}