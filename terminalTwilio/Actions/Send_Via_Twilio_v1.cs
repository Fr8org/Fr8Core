using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web.UI.WebControls;
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
using Twilio;
using Utilities;
using TextBox = Data.Control.TextBox;

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
        /*
        //this entire function gets passed as a delegate to the main processing code in the base class
        //currently many actions have two stages of configuration, and this method determines which stage should be applied
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO)) 
            { 
                return ConfigurationRequestType.Initial;
            }


            return ConfigurationRequestType.Followup;
        }
        */
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
            return await Task.FromResult(curActionDO);
        }
        private Crate PackCrate_ConfigurationControls()
        {
            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                new TextSource("SMS Number", "Upstream Terminal-Provided Fields", "SMS_Number"),
                new TextBox()
                {
                    Label = "SMS Body",
                    Name = "SMS_Body",
                    Required = true
                }
            };
            /*
            var controls = new StandardConfigurationControlsCM()
            {
                Controls = fieldsDTO
            };
             * */
            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private List<FieldDTO> GetRegisteredSenderNumbersData()
        {
            return _twilio.GetRegisteredSenderNumbers().Select(number => new FieldDTO() { Key = number, Value = number }).ToList();
        }

        /*
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
        }*/

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO,AuthorizationTokenDO authTokenDO)
        {
            //not currently any requirements that need attention at FollowupConfigurationResponse
            return await Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            Message curMessage;
            var processPayload = await GetProcessPayload(curActionDO, containerId);
            var controlsCrate = Crate.GetStorage(curActionDO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (controlsCrate == null)
            {
                PackCrate_WarningMessage(curActionDO, "No StandardConfigurationControlsCM crate provided", "No Controls");
                return null;
            }
            try
            {
                KeyValuePair<string, string> smsInfo = ParseSMSNumberAndMsg(controlsCrate);
                string smsNumber = smsInfo.Key;
                string smsBody = smsInfo.Value;

                if (String.IsNullOrEmpty(smsNumber))
                {
                    PackCrate_WarningMessage(curActionDO, "No SMS Number Provided", "No Number");
                    return null;
                }
                try
                {
                    curMessage = _twilio.SendSms(smsNumber, smsBody);
                    EventManager.TwilioSMSSent(smsNumber, smsBody);
                    var curFieldDTOList = CreateKeyValuePairList(curMessage);
                    using (var updater = Crate.UpdateStorage(processPayload))
                    {
                        updater.CrateStorage.Clear();
                        updater.CrateStorage.Add(PackCrate_TwilioMessageDetails(curFieldDTOList));
                    }
                }
                catch (Exception ex)
                {
                    EventManager.TwilioSMSSendFailure(smsNumber, smsBody, ex.Message);
                    PackCrate_WarningMessage(curActionDO, ex.Message, "Twilio Service Failure");
                }
            }
            catch (ArgumentException appEx)
            {
                PackCrate_WarningMessage(curActionDO, appEx.Message, "SMS Number");
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
            var standardControls = crateDTO.Get<StandardConfigurationControlsCM>();
            if (standardControls == null)
            {
                throw new ArgumentException("CrateDTO is not a standard UI control");
            }
            var smsBodyFields = standardControls.FindByName("SMS_Body");
            var smsNumber = GetSMSNumber((TextSource)standardControls.Controls[0]);
            return new KeyValuePair<string, string>(smsNumber, smsBodyFields.Value);
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
                    throw new ApplicationException("Could not extract number, unknown mode.");
            }
        }
        private List<FieldDTO> CreateKeyValuePairList(Message curMessage)
        {
            List<FieldDTO> returnList = new List<FieldDTO>();
            returnList.Add(new FieldDTO("Status", curMessage.Status));
            returnList.Add(new FieldDTO("ErrorMessage", curMessage.ErrorMessage));
            returnList.Add(new FieldDTO("Body", curMessage.Body));
            returnList.Add(new FieldDTO("ToNumber", curMessage.To));
            return returnList;
        }
        private Crate PackCrate_TwilioMessageDetails(List<FieldDTO> curTwilioMessage)
        {
            return Data.Crates.Crate.FromContent("Message Data", new StandardPayloadDataCM(curTwilioMessage));
        }

        private void PackCrate_WarningMessage(ActionDO actionDO, string warningMessage, string warningLabel)
        {
            var textBlock = new TextBlock
            {
                Label = warningLabel,
                Value = warningMessage,
                CssClass = "alert alert-warning"
            };
            using (var updater = Crate.UpdateStorage(actionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(textBlock));
            }
        }
    }
}