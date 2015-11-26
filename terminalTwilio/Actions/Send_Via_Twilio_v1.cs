using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Data.Constants;
using Data.Control;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.Infrastructure;
using Data.States;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalTwilio.Services;
using Utilities;

namespace terminalTwilio.Actions
{
    public class Send_Via_Twilio_v1 : BaseTerminalAction
    {
        protected ITwilioService _twilio;

        public Send_Via_Twilio_v1()
	    {
            _twilio = ObjectFactory.GetInstance<ITwilioService>();
	    }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDO, actionDO => ConfigurationRequestType.Initial, authTokenDO);
        }

        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
                return ConfigurationRequestType.Initial;
            else
                return ConfigurationRequestType.Followup;
        }
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                if (updater.CrateStorage.All(c => c.ManifestType.Id != (int)MT.StandardDesignTimeFields))
                {
                    var crateControlsDTO = PackCrate_ConfigurationControls();
                    updater.CrateStorage = new CrateStorage(crateControlsDTO);
                }
                var curUpstreamFieldsCrate = updater.CrateStorage.SingleOrDefault(c =>
                                                                                    c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                && c.Label == "Upstream Terminal-Provided Fields");
                if (curUpstreamFieldsCrate != null)
                {
                    updater.CrateStorage.Remove(curUpstreamFieldsCrate);
                }
                var curUpstreamFields = GetRegisteredSenderNumbersData().ToArray();
                curUpstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);
                updater.CrateStorage.Add(curUpstreamFieldsCrate);
            }
            return await Task.FromResult<ActionDO>(curActionDO);
        }
        private Crate PackCrate_ConfigurationControls()
        {
            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                new TextSource("For the SMS Number Use:", "Upstream Terminal-Provided Fields", "SMS_Number"),
                new TextBox()
            {
                Label = "SMS Body",
                Name = "SMS_Body",
                Required = true
            }
            };
            var controls = new StandardConfigurationControlsCM()
            {
                Controls = fieldsDTO
            };
            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private List<FieldDTO> GetRegisteredSenderNumbersData()
        {
            List<FieldDTO> phoneNumberFields = new List<FieldDTO>();

            phoneNumberFields = _twilio.GetRegisteredSenderNumbers().Select(number => new FieldDTO() { Key = number, Value = number }).ToList();

            return phoneNumberFields;
        }

        private Crate GetAvailableDataFields(ActionDO curActionDO)
        {
            Crate crate;

            var curUpstreamFields = GetRegisteredSenderNumbersData().ToArray();

            if (curUpstreamFields.Length == 0)
            {
                crate = PackCrate_ErrorTextBox("Error_NoUpstreamLists", "No Upstream fr8 Lists Were Found.");
                curActionDO.currentView = "Error_NoUpstreamLists";
            }
            else
            {
                crate = Crate.CreateDesignTimeFieldsCrate("Available Fields", curUpstreamFields);
            }

            return crate;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO,AuthorizationTokenDO authTokenDO)
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

        public async Task<PayloadDTO> Run(ActionDO curActionDO,
            Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var processPayload = await GetProcessPayload(curActionDO, containerId);

            var controlsCrate = Crate.GetStorage(curActionDO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
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
        public KeyValuePair<string, string> ParseSMSNumberAndMsg(Crate crateDTO)
        {
            KeyValuePair<string, string> smsInfo;

            var standardControls = crateDTO.Get<StandardConfigurationControlsCM>();
            if (standardControls == null)
            {
                throw new ArgumentException("CrateDTO is not a standard UI controls");
            }
            var smsBodyFields = standardControls.FindByName("SMS_Body");
            var smsNumber = GetSMSNumber((TextSource)standardControls.Controls[0]);
            smsInfo = new KeyValuePair<string, string>(smsNumber, smsBodyFields.Value);
            return smsInfo;
        }

        //private string GetSMSNumber(RadioButtonGroup radioButtonGroupControl)
        //{
        //    string smsNumber = "";

        //    var radioOptionSpecific = radioButtonGroupControl.Radios.Where(r => r.Controls.Where(c => c.Name == "SMS_Number").Count() > 0).FirstOrDefault();

        //    if (radioOptionSpecific.Selected) 
        //    {
        //        smsNumber = radioButtonGroupControl.Radios.SelectMany(s => s.Controls).Where(c => c.Name == "SMS_Number").Select(v => v.Value).FirstOrDefault();
        //    }
        //    else
        //    {
        //        smsNumber = radioButtonGroupControl.Radios.SelectMany(s => s.Controls).Where(c => c.Name == "upstream_crate").Select(v => v.Value).FirstOrDefault();
        //    }

        //    return smsNumber;
        //}

        private string GetSMSNumber(TextSource control)
        {
            string smsNumber = "";
            if (control == null)
            {
                throw new ApplicationException("TextSource control was expected but not found.");
            }
            switch (control.ValueSource)
            {
                case "specific":
                    return control.Value;

                case "upstream":
                    return control.Value;

                default:
                    throw new ApplicationException("Could not extract recipient, unknown recipient mode.");
            }
        }
    }
}