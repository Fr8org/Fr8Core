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
    public class Send_Via_Twilio_v1 : BaseTerminalActivity
    {
        protected ITwilioService _twilio;

        public Send_Via_Twilio_v1()
        {
            _twilio = ObjectFactory.GetInstance<ITwilioService>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActivityDO, EvaluateReceivedRequest, authTokenDO);
        }

        /// <summary>
        /// this entire function gets passed as a delegate to the main processing code in the base class
        /// currently many actions have two stages of configuration, and this method determines which stage should be applied
        /// </summary>
        private ConfigurationRequestType EvaluateReceivedRequest(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackCrate_ConfigurationControls());
                updater.CrateStorage.Add(await CreateAvailableFieldsCrate(curActivityDO));
            }
            return await Task.FromResult(curActivityDO);
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                CreateSpecificOrUpstreamValueChooser("SMS Number", "SMS_Number", "Upstream Terminal-Provided Fields"),
                CreateSpecificOrUpstreamValueChooser("SMS Body", "SMS_Body", "Upstream Terminal-Provided Fields")
            };

            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        private List<FieldDTO> GetRegisteredSenderNumbersData()
        {
            return _twilio.GetRegisteredSenderNumbers().Select(number => new FieldDTO() { Key = number, Value = number }).ToList();
        }

        /*
        private Crate GetAvailableDataFields(ActionDO curActivityDO)
        {
            Crate crate;

            var curUpstreamFields = GetRegisteredSenderNumbersData().ToArray();

            if (curUpstreamFields.Length == 0)
            {
                crate = PackCrate_ErrorTextBox("Error_NoUpstreamLists", "No Upstream fr8 Lists Were Found.");
                curActivityDO.currentView = "Error_NoUpstreamLists";
            }
            else
            {
                crate = Crate.CreateDesignTimeFieldsCrate("Available Fields", curUpstreamFields);
            }

            return crate;
        }*/

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //not currently any requirements that need attention at FollowupConfigurationResponse
            return await Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            Message curMessage;
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var controlsCrate = Crate.GetStorage(curActivityDO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (controlsCrate == null)
            {
                PackCrate_WarningMessage(curActivityDO, "No StandardConfigurationControlsCM crate provided", "No Controls");
                return Error(payloadCrates, "No StandardConfigurationControlsCM crate provided");
            }
            try
            {
                FieldDTO smsFieldDTO = ParseSMSNumberAndMsg(controlsCrate, payloadCrates);
                string smsNumber = smsFieldDTO.Key;
                string smsBody = smsFieldDTO.Value;

                if (String.IsNullOrEmpty(smsNumber))
                {
                    PackCrate_WarningMessage(curActivityDO, "No SMS Number Provided", "No Number");
                    return Error(payloadCrates, "No SMS Number Provided");
                }
                try
                {
                    curMessage = _twilio.SendSms(smsNumber, smsBody);
                    EventManager.TwilioSMSSent(smsNumber, smsBody);
                    var curFieldDTOList = CreateKeyValuePairList(curMessage);
                    using (var updater = Crate.UpdateStorage(payloadCrates))
                    {
                        updater.CrateStorage.Add(PackCrate_TwilioMessageDetails(curFieldDTOList));
                    }
                }
                catch (Exception ex)
                {
                    EventManager.TwilioSMSSendFailure(smsNumber, smsBody, ex.Message);
                    PackCrate_WarningMessage(curActivityDO, ex.Message, "Twilio Service Failure");
                    return Error(payloadCrates, "Twilio Service Failure");
                }
            }
            catch (ArgumentException appEx)
            {
                PackCrate_WarningMessage(curActivityDO, appEx.Message, "SMS Number");
                return Error(payloadCrates, appEx.Message);
            }
            return Success(payloadCrates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crateDTO"></param>
        /// <returns>Key = SMS Number; Value = SMS Body</returns>
        public FieldDTO ParseSMSNumberAndMsg(Crate crateDTO, PayloadDTO payloadCrates)
        {
            var standardControls = crateDTO.Get<StandardConfigurationControlsCM>();
            if (standardControls == null)
            {
                throw new ArgumentException("CrateDTO is not a standard UI control");
            }
            var smsNumber = GetSMSNumber((TextSource)standardControls.Controls[0], payloadCrates);
            var smsBody = GetSMSBody((TextSource)standardControls.Controls[1], payloadCrates);

            return new FieldDTO(smsNumber, smsBody);
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

        private string GetSMSNumber(TextSource control, PayloadDTO payloadCrates)
        {
            string smsNumber = "";
            if (control == null)
            {
                throw new ApplicationException("TextSource control was expected but not found.");
            }
            switch (control.ValueSource)
            {
                case "specific":
                    smsNumber = control.TextValue;
                    break;
                case "upstream":
                    //get the payload data 'Key' based on the selected control.Value and get its 'Value' from payload data
                    smsNumber = Crate.GetFieldByKey<StandardPayloadDataCM>(payloadCrates.CrateStorage, control.Value);
                    break;
                default:
                    throw new ApplicationException("Could not extract number, unknown mode.");
            }

            if (smsNumber.Trim().Length == 10 && !smsNumber.Contains("+"))
                smsNumber = "+1" + smsNumber;

            return smsNumber;
        }

        private string GetSMSBody(TextSource control, PayloadDTO payloadCrates)
        {

            string smsBody = "";
            if (control == null)
            {
                throw new ApplicationException("TextSource control was expected but not found.");
            }
            switch (control.ValueSource)
            {
                case "specific":
                    smsBody = control.TextValue;
                    break;
                case "upstream":
                    //get the payload data 'Key' based on the selected control.Value and get its 'Value' from payload data
                    smsBody = Crate.GetFieldByKey<StandardPayloadDataCM>(payloadCrates.CrateStorage, control.Value);
                    break;
                default:
                    throw new ApplicationException("Could not extract number, unknown mode.");
            }
            smsBody = "Fr8 Alert: " + smsBody + " For more info, visit http://fr8.co/sms";

            return smsBody;
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

        private void PackCrate_WarningMessage(ActivityDO activityDO, string warningMessage, string warningLabel)
        {
            var textBlock = GenerateTextBlock(warningLabel, warningMessage, "alert alert-warning");
            using (var updater = Crate.UpdateStorage(activityDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(textBlock));
            }
        }
    }
}