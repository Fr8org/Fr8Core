using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using pluginTwilio.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Data.Interfaces.ManifestSchemas;
using Core.Interfaces;
using Data.Infrastructure;
namespace pluginTwilio.Actions
{
    public class Send_Via_Twilio_v1 : BasePluginAction
    {
        protected ITwilioService _twilio;

        public Send_Via_Twilio_v1()
	    {
            _twilio = ObjectFactory.GetInstance<ITwilioService>();
	    }

        public override async Task<ActionDTO> Configure(ActionDTO curActionDTO)
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
            curActionDTO.CrateStorage.CrateDTO.Add(PackCrate_ConfigurationControls());
            curActionDTO.CrateStorage.CrateDTO.Add(GetAvailableDataFields(curActionDTO));
            return await Task.FromResult<ActionDTO>(curActionDTO);
        }

        private CrateDTO PackCrate_ConfigurationControls()
        {
            TextBoxControlDefinitionDTO smsNumberTextbox = new TextBoxControlDefinitionDTO()
            {
                Label = "SMS Number",
                Name = "SMS_Number",
                Required = true
            };
            RadioButtonOption specificNumberOption = new RadioButtonOption()
            {
                Selected = true,
                Name = "SMSNumberOption",
                Controls = new List<ControlDefinitionDTO>() { smsNumberTextbox }
            };


            //get data for combobox for upstream data
            var fieldSMSNumberLists = new DropDownListControlDefinitionDTO()
            {
                Label = "a value from Upstream Crate:",
                Name = "upstream_crate",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available SMS Number",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            RadioButtonOption upstreamOption = new RadioButtonOption()
            {
                Selected = false,
                Name = "SMSNumberOption",
                Controls = new List<ControlDefinitionDTO>() { fieldSMSNumberLists }
            };

            RadioButtonGroupControlDefinitionDTO radioGroup = new RadioButtonGroupControlDefinitionDTO()
            {
                GroupName = "SMSNumber_Group",
                Label = "For the SMS Number use:",
                Radios = new List<RadioButtonOption>() { specificNumberOption, upstreamOption }
            };

            TextBoxControlDefinitionDTO smsBody = new TextBoxControlDefinitionDTO()
            {
                Label = "SMS Body",
                Name = "SMS_Body",
                Required = true
            };

            return PackControlsCrate(radioGroup, smsBody);
        }

        private List<FieldDTO> GetRegisteredSenderNumbersData()
        {
            List<FieldDTO> phoneNumberFields = new List<FieldDTO>();

            phoneNumberFields = _twilio.GetRegisteredSenderNumbers().Select(number => new FieldDTO() { Key = number, Value = number }).ToList();

            return phoneNumberFields;
        }

        private CrateDTO GetAvailableDataFields(ActionDTO curActionDTO)
        {
            CrateDTO crateDTO = new CrateDTO();
            ActionDO curActionDO = _action.MapFromDTO(curActionDTO);
            var curUpstreamFields = GetRegisteredSenderNumbersData().ToArray();

            if (curUpstreamFields.Length == 0)
            {
                crateDTO = PackCrate_ErrorTextBox("Error_NoUpstreamLists",
                         "No Upstream fr8 Lists Were Found.");
                curActionDTO.CurrentView = "Error_NoUpstreamLists";
            }
            else
            {
                crateDTO = _crate.CreateDesignTimeFieldsCrate("Available Fields", curUpstreamFields);
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
            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);

            var controlsCrate = curActionDTO.CrateStorage.CrateDTO.FirstOrDefault();
            if (controlsCrate == null)
                return null;
        
            var standardControls = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(controlsCrate.Contents);
            if (standardControls == null)
                return null;

            var smsNumberFields = standardControls.FindByName("SMS_Number");
            var smsBodyFields = standardControls.FindByName("SMS_Body");
            
            if((smsNumberFields == null && smsBodyFields == null) &&
                (String.IsNullOrEmpty(smsNumberFields.Value)))
            {
                return null;
            }
            else
            {
                try
                {
                    _twilio.SendSms(smsNumberFields.Value, smsBodyFields.Value);
                    EventManager.TwilioSMSSent(smsNumberFields.Value, smsBodyFields.Value);
                }
                catch (Exception ex)
                {
                    EventManager.TwilioSMSSendFailure(smsNumberFields.Value, smsBodyFields.Value, ex.Message);
                }
            }

            return processPayload;
        }
    }
}