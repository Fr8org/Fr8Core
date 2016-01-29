using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

using Hub.Managers;
using TerminalBase.Infrastructure;
using Utilities;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v1 : BaseDocuSignAction
    {
        private DocuSignManager _docuSignManager = new DocuSignManager();
        public Send_DocuSign_Envelope_v1()
        {
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            var curEnvelope = new Envelope();
            curEnvelope.Login = new DocuSignPackager()
                .Login(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);

            curEnvelope = AddTemplateData(curActivityDO, payloadCrates, curEnvelope);
            curEnvelope.EmailSubject = "Test Message from Fr8";
            curEnvelope.Status = "sent";

            var result = curEnvelope.Create();

            return Success(payloadCrates);
        }

        private string ExtractTemplateId(ActivityDO curActivityDO)
        {
            var controls = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First().Controls;

            var templateDropDown = controls.SingleOrDefault(x => x.Name == "target_docusign_template");

            if (templateDropDown == null)
            {
                throw new ApplicationException("Could not find target_docusign_template DropDownList control.");
            }

            var result = templateDropDown.Value;
            return result;
        }

        private Envelope AddTemplateData(ActivityDO activityDO, PayloadDTO payloadCrates, Envelope curEnvelope)
        {
            var curTemplateId = ExtractTemplateId(activityDO);
            var payloadCrateStorage = Crate.GetStorage(payloadCrates);
            var configurationControls = GetConfigurationControls(activityDO);
            var recipientField = (TextSource)GetControl(configurationControls, "Recipient", ControlTypes.TextSource);

            var curRecipientAddress = recipientField.GetValue(payloadCrateStorage, true);

            curEnvelope.TemplateId = curTemplateId;
            curEnvelope.TemplateRoles = new TemplateRole[]
            {
                new TemplateRole()
                {
                    email = curRecipientAddress,
                    name = curRecipientAddress,
                    roleName = "Signer"   // need to fetch this
                },
            };

            return curEnvelope;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            // Do we have any crate? If no, it means that it's Initial configuration
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            // Try to find Configuration_Controls
            var stdCfgControlMS = Crate.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (stdCfgControlMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            // Try to get DropdownListField
            var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
            if (dropdownControlDTO == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var docusignTemplateId = dropdownControlDTO.Value;
            if (string.IsNullOrEmpty(docusignTemplateId))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                // Only do it if no existing MT.StandardDesignTimeFields crate is present to avoid loss of existing settings
                // Two crates are created
                // One to hold the ui controls
                if (updater.CrateStorage.All(c => c.ManifestType.Id != (int)MT.StandardDesignTimeFields))
                {
                    var crateControlsDTO = CreateDocusignTemplateConfigurationControls(curActivityDO);
                    // and one to hold the available templates, which need to be requested from docusign
                    var crateDesignTimeFieldsDTO = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);

                    updater.CrateStorage = new CrateStorage(crateControlsDTO, crateDesignTimeFieldsDTO);
                }

                await UpdateUpstreamCrate(curActivityDO, updater);
            }



            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                if (updater.CrateStorage.Count == 0)
                {
                    return curActivityDO;
                }

                await UpdateUpstreamCrate(curActivityDO, updater);

                // Try to find Configuration_Controls.
                var stdCfgControlMS = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                if (stdCfgControlMS == null)
                {
                    return curActivityDO;
                }

                // Try to find DocuSignTemplate drop-down.
                var dropdownControlDTO = stdCfgControlMS.FindByName("target_docusign_template");
                if (dropdownControlDTO == null)
                {
                    return curActivityDO;
                }

                // Get DocuSign Template Id
                var docusignTemplateId = dropdownControlDTO.Value;

                // Get Template
                var docuSignEnvelope = new DocuSignEnvelope(docuSignAuthDTO.Email, docuSignAuthDTO.ApiPassword);
                var envelopeDataDTO = docuSignEnvelope.GetEnvelopeDataByTemplate(docusignTemplateId).ToList();

                // when we're in design mode, there are no values
                // we just want the names of the fields
                var userDefinedFields = new List<FieldDTO>();
                envelopeDataDTO.ForEach(x => userDefinedFields.Add(new FieldDTO() { Key = x.Name, Value = x.Name, Availability = AvailabilityType.RunTime }));

                // we're in design mode, there are no values 
                var standartFields = new List<FieldDTO>()
                {
                    new FieldDTO() {Key = "recipient", Value = "recipient", Availability = AvailabilityType.RunTime }
                };

                var crateUserDefinedDTO = Crate.CreateDesignTimeFieldsCrate(
                    "DocuSignTemplateUserDefinedFields",
                    userDefinedFields.ToArray()
                );

                var crateStandardDTO = Crate.CreateDesignTimeFieldsCrate(
                    "DocuSignTemplateStandardFields",
                    standartFields.ToArray()
                );

                updater.CrateStorage.RemoveByLabel("DocuSignTemplateUserDefinedFields");
                updater.CrateStorage.RemoveByLabel("DocuSignTemplateStandardFields");
                updater.CrateStorage.Add(crateUserDefinedDTO);
                updater.CrateStorage.Add(crateStandardDTO);

            }

            return await Task.FromResult(curActivityDO);
        }

        private Crate CreateDocusignTemplateConfigurationControls(ActivityDO curActivityDO)
        {
            var fieldSelectDocusignTemplateDTO = new DropDownList()
            {
                Label = "Use DocuSign Template",
                Name = "target_docusign_template",
                Required = true,
                Events = new List<ControlEvent>()
                {
                     new ControlEvent("onChange", "requestConfig")
                },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };

            var fieldsDTO = new List<ControlDefinitionDTO>()
            {
                fieldSelectDocusignTemplateDTO,
                new TextSource("Email Address", "Upstream Terminal-Provided Fields", "Recipient")
                {
                    selectedKey = "Recipient",
                    ValueSource = "upstream"
                }
            };

            var controls = new StandardConfigurationControlsCM()
            {
                Controls = fieldsDTO
            };

            return Crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", fieldsDTO.ToArray());
        }

        public async Task UpdateUpstreamCrate(ActivityDO curActivityDO, ICrateStorageUpdater updater)
        {
            // Build a crate with the list of available upstream fields
            var curUpstreamFieldsCrate = updater.CrateStorage.SingleOrDefault(c => c.ManifestType.Id == (int)MT.StandardDesignTimeFields
                                                                                && c.Label == "Upstream Terminal-Provided Fields");

            if (curUpstreamFieldsCrate != null)
            {
                updater.CrateStorage.Remove(curUpstreamFieldsCrate);
            }

            var curUpstreamFields = (await GetDesignTimeFields(curActivityDO.Id, CrateDirection.Upstream))
                .Fields.Where(a => a.Availability == AvailabilityType.RunTime)
                .ToArray();

            //make fields inaccessible to up/downstanding actions
            curUpstreamFields.ToList().ForEach(a => a.Availability = AvailabilityType.Configuration);

            curUpstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Terminal-Provided Fields", curUpstreamFields);
            updater.CrateStorage.Add(curUpstreamFieldsCrate);
        }

    }
}