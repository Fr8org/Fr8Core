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
    public class CreateLauncher_v1 : BaseTerminalActivity
    {

        protected virtual async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
            }

            return curActivityDO;
        }

        protected virtual async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            
            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        private async Task LoadUserPlans()
        {

        }

        protected Crate CreateControlsCrate()
        {
            var infoText = new TextBlock()
            {
                Value = "Construct a Launcher that gathers information from users and passes it to another Plan",
                Name = "info_text"
            };

            var launcherName = new TextBox()
            {
                Label = "Launcher Name",
                Name = "launcher_name"
            };

            var targetPlan = new DropDownList()
            {
                Name = "target_plan",
                Label = "Target Plan",
                ListItems = new List<ListItem>() { }
            };

            var cc = new ControlContainer()
            {
                Label = "Please insert your desired controls below",
                Name = "control_container"
            };


            return PackControlsCrate(infoText, launcherName, targetPlan, cc);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}