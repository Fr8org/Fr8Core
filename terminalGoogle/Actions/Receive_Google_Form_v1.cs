using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerminalBase.BaseClasses;
using terminalGoogle.DataTransferObjects;
using Hub.Managers;
using terminalGoogle.Services;
using TerminalBase.Infrastructure;
using Data.Control;

namespace terminalGoogle.Actions
{
    public class Receive_Google_Form_v1 : BaseTerminalAction
    {
        GoogleDrive _googleDrive;
        public Receive_Google_Form_v1()
        {
            _googleDrive = new GoogleDrive();
        }

        protected bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (authTokenDO == null) return true;
            if (!base.NeedsAuthentication(authTokenDO))
                return false;
            var token = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // we may also post token to google api to check its validity
            return (token.Expires - DateTime.Now > TimeSpan.FromMinutes(5) ||
                    !string.IsNullOrEmpty(token.RefreshToken));
        }

        public override Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            base.CheckAuthentication(authTokenDO);

            return base.Configure(curActionDO, authTokenDO);
        }
            
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
                var configurationControlsCrate = PackCrate_ConfigurationControls();
                var crateDesignTimeFields = await PackCrate_GoogleForms(authDTO);
                var eventCrate = CreateEventSubscriptionCrate();

                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.Add(configurationControlsCrate);
                    updater.CrateStorage.Add(crateDesignTimeFields);
                    updater.CrateStorage.Add(eventCrate);
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
            return await Task.FromResult(curActionDO);
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectTemplate = new DropDownList()
            {
                Label = "Select Google Form",
                Name = "Selected_Google_Form",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Available Forms",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var controls = PackControlsCrate(fieldSelectTemplate);
            return controls;
        }

        private async Task<Crate> PackCrate_GoogleForms(GoogleAuthDTO authDTO)
        {
            if (string.IsNullOrEmpty(authDTO.RefreshToken))
                throw new ArgumentNullException("Token is empty");

            var files = await _googleDrive.GetGoogleForms(authDTO);

            var curFields = files.Select(file => new FieldDTO() { Key = file.Value, Value = file.Key }).ToArray();
            Crate crate = Crate.CreateDesignTimeFieldsCrate("Available Forms", curFields);

            return await Task.FromResult(crate);
        }

        private Crate CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Google Form Response"
            };

            return Crate.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                subscriptions.ToArray()
                );
        }

        public async Task<ActionDO> Activate(ActionDO curActionDTO, AuthorizationTokenDO authTokenDO)
        {
            var googleAuthDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);

            //get form id
            var controlsCrate = Crate.GetStorage(curActionDTO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            var standardControls = controlsCrate.Get<StandardConfigurationControlsCM>();

            var googleFormControl = standardControls.FindByName("Selected_Google_Form");

            var formId = googleFormControl.Value;

            if (string.IsNullOrEmpty(formId))
                throw new ArgumentNullException("Google Form selected is empty. Please select google form to receive.");

            var result = await _googleDrive.UploadAppScript(googleAuthDTO, formId);

            var fieldResult = new List<FieldDTO>(){ new FieldDTO(){ Key = result, Value = result}};
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Google Form Payload Data", new StandardPayloadDataCM(fieldResult)));
            }

            return await Task.FromResult(curActionDTO);
        }

        public async Task<ActionDO> Deactivate(ActionDO curActionDTO, AuthorizationTokenDO authTokenDO)
        {
            return await Task.FromResult(curActionDTO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            base.CheckAuthentication(authTokenDO);

            var processPayload = await GetProcessPayload(curActionDO, containerId);
            var payloadFields = ExtractPayloadFields(processPayload);
            var formResponseFields = CreatePayloadFormResponseFields(payloadFields);
            
            using (var updater = Crate.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Google Form Payload Data", new StandardPayloadDataCM(formResponseFields)));
            }

            return processPayload;
        }

        private List<FieldDTO> CreatePayloadFormResponseFields(List<FieldDTO> payloadfields)
        {
            List<FieldDTO> formFieldResponse = new List<FieldDTO>();
            string[] formresponses = payloadfields.FirstOrDefault(w => w.Key == "response").Value.Split(new char[] { '&' });

            if (formresponses.Length > 0)
            {
                formresponses[formresponses.Length - 1] = formresponses[formresponses.Length - 1].TrimEnd(new char[] { '&' });

                foreach (var response in formresponses)
                {
                    string[] itemResponse = response.Split(new char[] { '=' });

                    if (itemResponse.Length >= 2)
                    {
                        formFieldResponse.Add(new FieldDTO() { Key = itemResponse[0], Value = itemResponse[1] });
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("No payload fields extracted");
            }

            return formFieldResponse;
        }

        private List<FieldDTO> ExtractPayloadFields(PayloadDTO processPayload)
        {
            var eventReportMS = Crate.GetStorage(processPayload).CrateContentsOfType<EventReportCM>().SingleOrDefault();
            if (eventReportMS == null)
            {
                throw new ApplicationException("EventReportCrate is empty.");
            }

            var eventFieldsCrate = eventReportMS.EventPayload.SingleOrDefault();
            if (eventFieldsCrate == null)
            {
                throw new ApplicationException("EventReportMS.EventPayload is empty.");
            }

            return eventReportMS.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().SelectMany(x => x.AllValues()).ToList();
        }
    }
}