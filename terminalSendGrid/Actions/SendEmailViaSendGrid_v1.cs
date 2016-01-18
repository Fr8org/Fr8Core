using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;

using Hub.Managers;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using Utilities;
using terminalSendGrid.Infrastructure;
using Data.Interfaces.Manifests;
using System.Linq;
namespace terminalSendGrid.Actions
{
    public class SendEmailViaSendGrid_v1 : BaseTerminalAction
    {
        // moved the EmailPackager ObjectFactory here since the basepluginAction will be called by others and the dependency is defiend in pluginsendGrid
        private IConfigRepository _configRepository;
        private IEmailPackager _emailPackager;
        private readonly  List<string> _excludedCrates;

        public SendEmailViaSendGrid_v1()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _emailPackager = ObjectFactory.GetInstance<IEmailPackager>();
            _excludedCrates = new List<string>() { "AvailableActions", "Available Templates" };
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDO, EvaluateReceivedRequest, authTokenDO);
        }

        /// <summary>
        /// this entire function gets passed as a delegate to the main processing code in the base class
        /// currently many actions have two stages of configuration, and this method determines which stage should be applied
        /// </summary>
        private ConfigurationRequestType EvaluateReceivedRequest(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(CreateControlsCrate());
                updater.CrateStorage.Add(await CreateAvailableFieldsCrate(curActionDO));
            }

            return await Task.FromResult(curActionDO);
        }

        protected async override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActionDO));
            }

            return await Task.FromResult(curActionDO);
        }

        // @alexavrutin here: Do we really need a separate crate for each field? 
        // Refactored the action to use a single Upstream Terminal-Provided Fields crate.
        private async Task<ActionDO> AddDesignTimeFieldsSource(ActionDO curActionDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.RemoveByLabel("Upstream Terminal-Provided Fields Address");
                updater.CrateStorage.RemoveByLabel("Upstream Terminal-Provided Fields Subject");
                updater.CrateStorage.RemoveByLabel("Upstream Terminal-Provided Fields Body");

                var fieldsDTO = await GetCratesFieldsDTO<StandardDesignTimeFieldsCM>(curActionDO, CrateDirection.Upstream);

                var upstreamFieldsAddress = MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActionDO, "Upstream Terminal-Provided Fields Address", fieldsDTO);
                if (upstreamFieldsAddress != null)
                    updater.CrateStorage.Add(upstreamFieldsAddress);

                var upstreamFieldsSubject = MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActionDO, "Upstream Terminal-Provided Fields Subject", fieldsDTO);
                if (upstreamFieldsSubject != null)
                    updater.CrateStorage.Add(upstreamFieldsSubject);

                var upstreamFieldsBody = MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActionDO, "Upstream Terminal-Provided Fields Body", fieldsDTO);
                if (upstreamFieldsBody != null)
                    updater.CrateStorage.Add(upstreamFieldsBody);
            }

            return curActionDO;
        }

        /// <summary>
        /// Create EmailAddress RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailAddressTextSourceControl()
        {
            var control = CreateSpecificOrUpstreamValueChooser("Email Address", "EmailAddress", "Upstream Terminal-Provided Fields", "EmailAddress");
                
            //CreateSpecificOrUpstreamValueChooser(
            //    "Email Address",
            //    "EmailAddress",
            //    "Upstream Terminal-Provided Fields Address",
            //    "EmailAddress"
            //);

            return control;
        }

        /// <summary>
        /// Create EmailSubject RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailSubjectTextSourceControl()
        {
            var control = CreateSpecificOrUpstreamValueChooser(
                "Email Subject",
                "EmailSubject",
                "Upstream Terminal-Provided Fields"
            );

            return control;
        }

        /// <summary>
        /// Create EmailBody RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailBodyTextSourceControl()
        {
            var control = CreateSpecificOrUpstreamValueChooser(
                "Email Body",
                "EmailBody",
                "Upstream Terminal-Provided Fields"
            );

            return control;
        }

        private Crate CreateControlsCrate()
        {
            var controls = new List<ControlDefinitionDTO>()
            {
                CreateEmailAddressTextSourceControl(),
                CreateEmailSubjectTextSourceControl(),
                CreateEmailBodyTextSourceControl()
            };

            return Crate.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel, controls.ToArray());
        }

        private string CreateEmailHTMLText(string emailBody)
        {
            var template = @"<html><body>{0}</body></html>";
            var htmlText = string.Format(template, emailBody);

            return htmlText;
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var fromAddress = _configRepository.Get("OutboundFromAddress");

            var payloadCrates = await GetPayload(curActionDO, containerId);

            var payloadCrateStorage = Crate.GetStorage(payloadCrates);
            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActionDO);

            // A fix to support an old (wrong) crate label (FR-1972). The following block can be savely removed in Feb 2016
            if (configurationControls == null)
            {
                var storage = Crate.GetStorage(curActionDO);
                configurationControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => String.Equals(c.Label, "SendGrid", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            var emailAddressField = (TextSource)GetControl(configurationControls, "EmailAddress", ControlTypes.TextSource);
            var emailSubjectField = (TextSource)GetControl(configurationControls, "EmailSubject", ControlTypes.TextSource);
            var emailBodyField = (TextSource)GetControl(configurationControls, "EmailBody", ControlTypes.TextSource);

            var emailAddress = emailAddressField.GetValue(payloadCrateStorage);
            var emailSubject = emailSubjectField.GetValue(payloadCrateStorage);
            var emailBody = emailBodyField.GetValue(payloadCrateStorage);

            var mailerDO = new TerminalMailerDO()
            {
                Email = new EmailDO()
                {
                    From = new EmailAddressDO
                    {
                        Address = fromAddress,
                        Name = "Fr8 Operations"
                    },

                    Recipients = new List<RecipientDO>()
                    {
                        new RecipientDO()
                        {
                            EmailAddress = new EmailAddressDO(emailAddress),
                            EmailParticipantType = EmailParticipantType.To
                        }
                    },
                    Subject = emailSubject,
                    HTMLText = CreateEmailHTMLText(emailBody)
                }
            };

            await _emailPackager.Send(mailerDO);

            return Success(payloadCrates);
        }
    }
}