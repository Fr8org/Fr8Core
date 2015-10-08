using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using pluginTwilio.Services;
using StructureMap;
using System;
using System.Threading.Tasks;

namespace pluginTwilio.Actions
{
    public class Send_Via_Twilio_v1 : BasePluginAction
    {
        protected ITwilioService _twilio;

        public Send_Via_Twilio_v1 ()
	    {
            _twilio = ObjectFactory.GetInstance<ITwilioService>();
	    }

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await ProcessConfigurationRequest(curActionDTO, actionDo => ConfigurationRequestType.Initial);
        }

        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;
            else
                return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null)
            {
                curActionDTO.CrateStorage = new CrateStorageDTO();
            }
            curActionDTO.CrateStorage.CrateDTO.Add(CreateControlsCrate());
            //commeted first for the avialble datafields for the dropdownlist to wait for the Nested Controls to be implemented
            //curActionDTO.CrateStorage.CrateDTO.Add(await GetAvailableDataFields(curActionDTO));
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        private CrateDTO CreateControlsCrate()
        {

            ControlsDefinitionDTO[] controls = 
            {
                new ControlsDefinitionDTO(ControlsDefinitionDTO.TEXTBOX_FIELD)
                {
                    Label = "SMS Number",
                    Name = "SMS_Number",
                    Required = true,
                },
               new ControlsDefinitionDTO(ControlsDefinitionDTO.TEXTBOX_FIELD)
                {
                    Label = "SMS Body",
                    Name = "SMS_Body",
                    Required = true,
                }
            };
            return _crate.CreateStandardConfigurationControlsCrate("Send SMS", controls);
        }

        private async Task<CrateDTO> GetAvailableDataFields(ActionDTO curActionDTO)
        {
            CrateDTO crateDTO = null;
            ActionDO curActionDO = _action.MapFromDTO(curActionDTO);
            var curUpstreamFields = (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Upstream)).Fields.ToArray();

            if (curUpstreamFields.Length == 0)
            {
                crateDTO = GetTextBoxControlForDisplayingError("Error_NoUpstreamLists",
                         "No Upstream fr8 Lists Were Found.");
                curActionDTO.CurrentView = "Error_NoUpstreamLists";
            }
            else
            {
                crateDTO = _crate.CreateDesignTimeFieldsCrate("Upstream Plugin-Provided Fields", curUpstreamFields);
            }

            return crateDTO;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            //not currently any requirements that need attention at FollowupConfigurationResponse
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        public object Activate(ActionDO curActionDO)
        {
            //not currently any requirements that need attention at Activation Time
            return curActionDO;
        }

        public object Deactivate(ActionDO curActionDO)
        {
            return curActionDO;
        }

        public async Task<PayloadDTO> Execute(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);

            return processPayload;
        }
    }
}