using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using terminalUtilities.Infrastructure;
using terminalUtilities.Interfaces;
using terminalUtilities.Models;
using System;

namespace terminalSendGrid.Activities
{
    public class Send_Email_Via_SendGrid_v1 : ExplicitTerminalActivity
    {
        private readonly IConfigRepository _configRepository;
        private readonly IEmailPackager _emailPackager;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("f827af1c-3348-4981-bebd-cf81c8ab27ae"),
            Name = "Send_Email_Via_SendGrid",
            Label = "Send Email Using SendGrid Account",
            Version = "1",
            Tags = string.Join(",", Tags.Notifier, Tags.EmailDeliverer),
            Terminal = TerminalData.TerminalDTO,
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Send_Email_Via_SendGrid_v1(ICrateManager crateManager, IConfigRepository configRepository, IEmailPackager emailPackager)
            : base(crateManager)
        {
            _configRepository = configRepository;
            _emailPackager = emailPackager;
        }


        public override async Task Initialize()
        {
            Storage.Clear();

            CreateControls();

            await Task.FromResult(0);
        }

        /// <summary>
        /// Create EmailAddress RadioButtonGroup
        /// </summary>
        /// <returns></returns>
        private ControlDefinitionDTO CreateEmailAddressTextSourceControl()
        {
            var control = UiBuilder.CreateSpecificOrUpstreamValueChooser(
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
            var control = UiBuilder.CreateSpecificOrUpstreamValueChooser(
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
            var control = UiBuilder.CreateSpecificOrUpstreamValueChooser(
                "Email Body",
                "EmailBody",
                addRequestConfigEvent: true,
                requestUpstream: true
            );

            return control;
        }

        private void CreateControls()
        {
            var controls = new List<ControlDefinitionDTO>()
            {
                CreateEmailAddressTextSourceControl(),
                CreateEmailSubjectTextSourceControl(),
                CreateEmailBodyTextSourceControl()
            };

            AddControls(controls);
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

            var emailAddress = emailAddressField.TextValue;
            var emailSubject = emailSubjectField.TextValue;
            var emailBody = emailBodyField.TextValue;

            var userData = await HubCommunicator.GetCurrentUser();
            var footerMessage = string.Format("<hr> <p> This email was generated by The Fr8 Company as part of the processing of Fr8 Container {0} on behalf of Fr8 User {1}." +
                                             "For questions about Fr8, go to fr8.co </p>", ExecutionContext.ContainerId, userData.FirstName + " " + userData.LastName);

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