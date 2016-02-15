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
    public class SendEmailViaSendGrid_v1 : BaseTerminalActivity
    {
        // moved the EmailPackager ObjectFactory here since the basepluginAction will be called by others and the dependency is defiend in pluginsendGrid
        private IConfigRepository _configRepository;
        private IEmailPackager _emailPackager;
        private readonly  List<string> _excludedCrates;

        public SendEmailViaSendGrid_v1()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _emailPackager = ObjectFactory.GetInstance<IEmailPackager>();
            _excludedCrates = new List<string>() { "AvailableActions" };
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

            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(CreateControlsCrate());
                crateStorage.Add(await CreateAvailableFieldsCrate(curActivityDO));
            }

            return await Task.FromResult(curActivityDO);
        }

        protected async override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.ReplaceByLabel(await CreateAvailableFieldsCrate(curActivityDO));
            }

            return await Task.FromResult(curActivityDO);
        }

        // @alexavrutin here: Do we really need a separate crate for each field? 
        // Refactored the action to use a single Upstream Terminal-Provided Fields crate.
        private async Task<ActivityDO> AddDesignTimeFieldsSource(ActivityDO curActivityDO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("Upstream Terminal-Provided Fields Address");
                crateStorage.RemoveByLabel("Upstream Terminal-Provided Fields Subject");
                crateStorage.RemoveByLabel("Upstream Terminal-Provided Fields Body");

                var fieldsDTO = await GetCratesFieldsDTO<StandardDesignTimeFieldsCM>(curActivityDO, CrateDirection.Upstream);

                var upstreamFieldsAddress = MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActivityDO, "Upstream Terminal-Provided Fields Address", fieldsDTO);
                if (upstreamFieldsAddress != null)
                    crateStorage.Add(upstreamFieldsAddress);

                var upstreamFieldsSubject = MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActivityDO, "Upstream Terminal-Provided Fields Subject", fieldsDTO);
                if (upstreamFieldsSubject != null)
                    crateStorage.Add(upstreamFieldsSubject);

                var upstreamFieldsBody = MergeUpstreamFields<StandardDesignTimeFieldsCM>(curActivityDO, "Upstream Terminal-Provided Fields Body", fieldsDTO);
                if (upstreamFieldsBody != null)
                    crateStorage.Add(upstreamFieldsBody);
            }

            return curActivityDO;
        }

        /// <summary>
        /// Create EmailAddress RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailAddressTextSourceControl()
        {
            var control = CreateSpecificOrUpstreamValueChooser("Email Address", "EmailAddress", "Upstream Terminal-Provided Fields");
                
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

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var fromAddress = _configRepository.Get("OutboundFromAddress");

            var payloadCrates = await GetPayload(curActivityDO, containerId);

            var payloadCrateStorage = Crate.GetStorage(payloadCrates);
            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActivityDO);

            // A fix to support an old (wrong) crate label (FR-1972). The following block can be savely removed in Feb 2016
            if (configurationControls == null)
            {
                var storage = Crate.GetStorage(curActivityDO);
                configurationControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => String.Equals(c.Label, "SendGrid", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            var emailAddressField = (TextSource)GetControl(configurationControls, "EmailAddress", ControlTypes.TextSource);
            var emailSubjectField = (TextSource)GetControl(configurationControls, "EmailSubject", ControlTypes.TextSource);
            var emailBodyField = (TextSource)GetControl(configurationControls, "EmailBody", ControlTypes.TextSource);

            var emailAddress = emailAddressField.GetValue(payloadCrateStorage);
            var emailSubject = emailSubjectField.GetValue(payloadCrateStorage);
            var emailBody =  emailBodyField.GetValue(payloadCrateStorage);

            var userData = await GetCurrentUserData(curActivityDO, containerId);
            var footerMessage = string.Format("<hr> <p> This email was generated by The Fr8 Company as part of the processing of Fr8 Container {0} on behalf of Fr8 User {1}." +
                                              "For questions about this email or other Fr8 matters, go to www.fr8.co </p>", containerId, userData.FirstName+" "+ userData.LastName);
            
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
                },
                Footer = footerMessage,
            };

            await _emailPackager.Send(mailerDO);

            return Success(payloadCrates);
        }
    }
}