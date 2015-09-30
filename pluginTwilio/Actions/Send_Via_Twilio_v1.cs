using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;

namespace pluginTwilio.Actions
{
    public class Send_Via_Twilio_v1 : BasePluginAction
    {
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public ActionDTO Configure(ActionDTO curActionDto)
        {
            return ProcessConfigurationRequest(curActionDto, actionDo => ConfigurationRequestType.Initial);
        }

        /**********************************************************************************/

        public void Activate(ActionDTO curActionDto)
        {
        }

        /**********************************************************************************/

        public void Deactivate(ActionDTO curActionDto)
        {
        }

        /**********************************************************************************/

        protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDto)
        {
            var textBlock = new TextBlockFieldDTO
            {
                FieldLabel = "Message to send",
                Value = "This Action doesn't require any configuration.",
                Type = "textBlockField",
                cssClass = "well well-lg"
            };

            var crateControls = PackControlsCrate(textBlock);

            curActionDto.CrateStorage.CrateDTO.Add(crateControls);
            return curActionDto;
        }

        /**********************************************************************************/

        public void Execute(ActionDataPackageDTO curActionDataPackageDto)
        {
        }

        /**********************************************************************************/
    }
}