using System;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class BuildQuery_v1 : BasePluginAction
    {
        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null
                || curActionDTO.CrateStorage.CrateDTO == null
                || curActionDTO.CrateStorage.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(
            ActionDTO curActionDTO)
        {
            throw new NotImplementedException();
        }

        protected override Task<ActionDTO> FollowupConfigurationResponse(
            ActionDTO curActionDTO)
        {
            throw new NotImplementedException();
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