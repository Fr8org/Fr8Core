using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.States;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;
using terminalUtilities.SendGrid;

namespace terminalSendGrid.Actions
{
    public class SendEmailViaSendGrid_v1 : BaseTerminalActivity
    {
        // moved the EmailPackager ObjectFactory here since the basepluginAction will be called by others and the dependency is defiend in pluginsendGrid
        private IConfigRepository _configRepository;
        private IEmailPackager _emailPackager;

        public SendEmailViaSendGrid_v1()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _emailPackager = ObjectFactory.GetInstance<IEmailPackager>();
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
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(CreateControlsCrate());
            }

            return await Task.FromResult(curActivityDO);
        }

        protected async override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            return await Task.FromResult(curActivityDO);
        }

        /// <summary>
        /// Create EmailAddress RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailAddressTextSourceControl()
        {
            var control = CreateSpecificOrUpstreamValueChooser(
                "Email Address",
                "EmailAddress",
                addRequestConfigEvent: true,
                requestUpstream: true
            );

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
                addRequestConfigEvent: true,
                requestUpstream: true
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
                addRequestConfigEvent: true,
                requestUpstream: true
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

            return CrateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel, controls.ToArray());
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

            var payloadCrateStorage = CrateManager.GetStorage(payloadCrates);
            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActivityDO);
            
            var emailAddressField = (TextSource)GetControl(configurationControls, "EmailAddress", ControlTypes.TextSource);
            var emailSubjectField = (TextSource)GetControl(configurationControls, "EmailSubject", ControlTypes.TextSource);
            var emailBodyField = (TextSource)GetControl(configurationControls, "EmailBody", ControlTypes.TextSource);

            var emailAddress = emailAddressField.GetValue(payloadCrateStorage);
            var emailSubject = emailSubjectField.GetValue(payloadCrateStorage);
            var emailBody =  emailBodyField.GetValue(payloadCrateStorage);

            var userData = await GetCurrentUserData(curActivityDO, containerId);
            var footerMessage = string.Format("<hr> <p> This email was generated by The Fr8 Company as part of the processing of Fr8 Container {0} on behalf of Fr8 User {1}." +
                                              "For questions about this email or other Fr8 matters, go to www.fr8.co </p>", containerId, userData.FirstName+" "+ userData.LastName);
            
            var mailerDO = new MailerDO()
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