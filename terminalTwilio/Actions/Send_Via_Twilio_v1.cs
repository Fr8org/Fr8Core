using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.Infrastructure;
using terminalTwilio.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Data.Interfaces;
using Data.Infrastructure;
using TerminalBase.BaseClasses;

namespace terminalTwilio.Actions
{
    public class Send_Via_Twilio_v1 : BasePluginAction
    {
        protected ITwilioService _twilio;

        public Send_Via_Twilio_v1()
	    {
            _twilio = ObjectFactory.GetInstance<ITwilioService>();
	    }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            return await ProcessConfigurationRequest(curActionDO, actionDO => ConfigurationRequestType.Initial, authTokenDO);
        }

        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDO curActionDO)
        {
            CrateStorageDTO curCrates = curActionDO.CrateStorageDTO();

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;
            else
                return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            if (curActionDO.CrateStorageDTO() == null)
            {
                curActionDO.UpdateCrateStorageDTO(new List<CrateDTO>());
            }
            curActionDO.CrateStorageDTO().CrateDTO.Add(PackCrate_ConfigurationControls());
            curActionDO.CrateStorageDTO().CrateDTO.Add(GetAvailableDataFields(curActionDO));
            return await Task.FromResult<ActionDO>(curActionDO);
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
                Controls = new List<ControlDefinitionDTO> { smsNumberTextbox }
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
                    Label = "Available Fields",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            RadioButtonOption upstreamOption = new RadioButtonOption()
            {
                Selected = false,
                Name = "SMSNumberOption",
                Controls = new List<ControlDefinitionDTO> { fieldSMSNumberLists }
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

        private CrateDTO GetAvailableDataFields(ActionDO curActionDO)
        {
            CrateDTO crateDTO = new CrateDTO();

            var curUpstreamFields = GetRegisteredSenderNumbersData().ToArray();

            if (curUpstreamFields.Length == 0)
            {
                crateDTO = PackCrate_ErrorTextBox("Error_NoUpstreamLists",
                            "No Upstream fr8 Lists Were Found.");
                curActionDO.currentView = "Error_NoUpstreamLists";
            }
            else
            {
                crateDTO = Crate.CreateDesignTimeFieldsCrate("Available Fields", curUpstreamFields);
            }

            return crateDTO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO,AuthorizationTokenDO authTokenDO=null)
        {
            //not currently any requirements that need attention at FollowupConfigurationResponse
            return await Task.FromResult<ActionDO>(curActionDO);
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

        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            var processPayload = await GetProcessPayload(containerId);

            var controlsCrate = curActionDO.CrateStorageDTO().CrateDTO.FirstOrDefault();
            if (controlsCrate == null)
                return null;

            var smsInfo = ParseSMSNumberAndMsg(controlsCrate);

            string smsNumber = smsInfo.Key;
            string smsBody = smsInfo.Value;

            if (String.IsNullOrEmpty(smsNumber))
            {
                return null;
            }
            else
            {
                try
                {
                    _twilio.SendSms(smsNumber, smsBody);
                    EventManager.TwilioSMSSent(smsNumber, smsBody);
                }
                catch (Exception ex)
                {
                    EventManager.TwilioSMSSendFailure(smsNumber, smsBody, ex.Message);
                }
            }

            return processPayload;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crateDTO"></param>
        /// <returns>Key = SMS Number; Value = SMS Body</returns>
        public KeyValuePair<string, string> ParseSMSNumberAndMsg(CrateDTO crateDTO)
        {
            KeyValuePair<string, string> smsInfo;

            var standardControls = Crate.GetStandardConfigurationControls(crateDTO);
            if (standardControls == null)
                throw new ArgumentException("CrateDTO is not a standard configuration controls");

            var smsBodyFields = standardControls.FindByName("SMS_Body");

            var smsNumber = GetSMSNumber((RadioButtonGroupControlDefinitionDTO)standardControls.Controls[0]);

            smsInfo = new KeyValuePair<string, string>(smsNumber, smsBodyFields.Value);

            return smsInfo;
        }

        private string GetSMSNumber(RadioButtonGroupControlDefinitionDTO radioButtonGroupControl)
        {
            string smsNumber = "";

            var radioOptionSpecific = radioButtonGroupControl.Radios.Where(r => r.Controls.Where(c => c.Name == "SMS_Number").Count() > 0).FirstOrDefault();

            if (radioOptionSpecific.Selected) 
            {
                smsNumber = radioButtonGroupControl.Radios.SelectMany(s => s.Controls).Where(c => c.Name == "SMS_Number").Select(v => v.Value).FirstOrDefault();
            }
            else
            {
                smsNumber = radioButtonGroupControl.Radios.SelectMany(s => s.Controls).Where(c => c.Name == "upstream_crate").Select(v => v.Value).FirstOrDefault();
            }

            return smsNumber;
        }
    }
}