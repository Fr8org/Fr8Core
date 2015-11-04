using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class ExecuteSql_v1 : BasePluginAction
    {
        #region Configuration

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            AddLabelControl(
                curActionDTO,
                "NoConfigLabel",
                "No configuration",
                "This action does not require any configuration."
            );

            return Task.FromResult(curActionDTO);
        }

        #endregion Configuration

        #region Execution

        public Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return Task.FromResult<PayloadDTO>(null);
        }

        #endregion Execution
    }
}