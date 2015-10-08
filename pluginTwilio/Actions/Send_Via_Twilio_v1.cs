using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using System.Threading.Tasks;

namespace pluginTwilio.Actions
{
    public class Send_Via_Twilio_v1 : BasePluginAction
    {
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationRequestType.Initial);
        }

        /**********************************************************************************/

        public void Activate(ActionDTO curActionDTO)
        {
        }

        /**********************************************************************************/

        public void Deactivate(ActionDTO curActionDTO)
        {
        }

        /**********************************************************************************/

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var textBlock = new ControlsDefinitionDTO(ControlsDefinitionDTO.TEXTBOX_FIELD)
            {
                Label = "Message to send",
                Value = "This Action doesn't require any configuration.",
            };

            var crateControls = PackControlsCrate(textBlock);

            curActionDTO.CrateStorage.CrateDTO.Add(crateControls);
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        /**********************************************************************************/

        public void Execute(ActionDataPackageDTO curActionDataPackageDTO)
        {
        }

        /**********************************************************************************/
    }
}