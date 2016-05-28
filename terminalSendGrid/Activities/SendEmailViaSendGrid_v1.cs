using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.States;
using terminalUtilities.Infrastructure;
using terminalUtilities.Interfaces;
using terminalUtilities.Models;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalSendGrid.Activities
{
    public class SendEmailViaSendGrid_v1 : BaseTerminalActivity
    {
        private readonly IConfigRepository _configRepository;
        private readonly IEmailPackager _emailPackager;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "SendEmailViaSendGrid",
            Label = "Send Email",
            Version = "1",
            Tags = string.Join(",", Tags.Notifier, Tags.EmailDeliverer),
            Terminal = TerminalData.TerminalDTO,
            Category = ActivityCategory.Forwarders,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public SendEmailViaSendGrid_v1(ICrateManager crateManager, IConfigRepository configRepository, IEmailPackager emailPackager)
            : base(false, crateManager)
        {
            _configRepository = configRepository;
            _emailPackager = emailPackager;
        }


        public override async Task Initialize()
        {
            Storage.Clear();
            Storage.Add(CreateControlsCrate());
            await Task.FromResult(0);
        }

        /// <summary>
        /// Create EmailAddress RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailAddressTextSourceControl()
        {
            var control = ControlHelper.CreateSpecificOrUpstreamValueChooser(
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
            var control = ControlHelper.CreateSpecificOrUpstreamValueChooser(
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
            var control = ControlHelper.CreateSpecificOrUpstreamValueChooser(
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

        protected override Task Validate()
        {
            var emailAddressField = GetControl<TextSource>("EmailAddress");
            var emailSubjectField = GetControl<TextSource>("EmailSubject");
            var emailBodyField = GetControl<TextSource>("EmailBody");
            ValidationManager.ValidateTextSourceNotEmpty(emailAddressField, "Email address can't be empty");
            ValidationManager.ValidateTextSourceNotEmpty(emailSubjectField, "Email subject can't be empty");
            ValidationManager.ValidateTextSourceNotEmpty(emailBodyField, "Email body can't be empty");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var fromAddress = _configRepository.Get("OutboundFromAddress");

            var emailAddressField = GetControl<TextSource>("EmailAddress");
            var emailSubjectField = GetControl<TextSource>("EmailSubject");
            var emailBodyField = GetControl<TextSource>("EmailBody");

            var emailAddress = emailAddressField.GetValue(Payload);
            var emailSubject = emailSubjectField.GetValue(Payload);
            var emailBody = emailBodyField.GetValue(Payload);

            var userData = await HubCommunicator.GetCurrentUser();
            var footerMessage = string.Format("<hr> <p> This email was generated by The Fr8 Company as part of the processing of Fr8 Container {0} on behalf of Fr8 User {1}." +
                                              "For questions about this email or other Fr8 matters, go to www.fr8.co </p>", ExecutionContext.ContainerId, userData.FirstName + " " + userData.LastName);

            var mailerDO = new TerminalMailerDO()
            {
                Email = new EmailDTO()
                {
                    From = new EmailAddressDTO
                    {
                        Address = fromAddress,
                        Name = "Fr8 Operations"
                    },

                    Recipients = new List<RecipientDTO>()
                    {
                        new RecipientDTO()
                        {
                            EmailAddress = new EmailAddressDTO(emailAddress),
                            EmailParticipantType = EmailParticipantType.To
                        }
                    },
                    Subject = emailSubject,
                    HTMLText = CreateEmailHTMLText(emailBody)
                },
                Footer = footerMessage,
            };

            await _emailPackager.Send(mailerDO);

            Success();
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }
    }
}