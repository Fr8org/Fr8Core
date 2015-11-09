using System;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class ManageRoute_v1 : BasePluginAction
    {
        private readonly FindObjectHelper _findObjectHelper = new FindObjectHelper();


        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null 
                || curActionDTO.CrateStorage.Crates == null
                || curActionDTO.CrateStorage.Crates.Length == 0)
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                var crateStorage = updater.CrateStorage;
                AddRunNowButton(crateStorage);
            }

            return Task.FromResult(curActionDTO);
        }

        private void AddRunNowButton(CrateStorage crateStorage)
        {
            AddControl(crateStorage, new ButtonControlDefinitionDTO()
            {
                Name = "RunNowButton",
                Label = "Run Now"
            });
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                var runNowButton = FindControl(updater.CrateStorage, "RunNowButton") as ButtonControlDefinitionDTO;
                if (runNowButton == null)
                {
                    throw new ApplicationException("RunNowButton was not found.");
                }

                if (runNowButton.Clicked)
                {
                    var routeId = await GetRouteId(curActionDTO);
                    if (routeId == 0)
                    {
                        throw new ApplicationException("No route found for current Action.");
                    }

                    await _findObjectHelper.LaunchContainer(routeId);
                }
            }

            return curActionDTO;
        }

        #endregion Configuration.


        #region Execution.

        public Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return Task.FromResult<PayloadDTO>(null);
        }

        #endregion Execution.
    }
}