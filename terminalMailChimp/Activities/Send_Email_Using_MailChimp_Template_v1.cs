using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Services;
using terminalMailChimp.Interfaces;

namespace terminalMailChimp.Activities
{
    public class Send_Email_Using_MailChimp_Template_v1 : TerminalActivity<Send_Email_Using_MailChimp_Template_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("fb03d117-244b-4937-9766-25f3c78bd68d"),
            Name = "Send_Email_Using_MailChimp_Template",
            Label = "Send Email Using MailChimp Template",
            Version = "1",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private readonly IMailChimpIntegration _mailChimpIntegration;

        private string SelectedMailChimpTemplate
        {
            get { return this[nameof(SelectedMailChimpTemplate)]; }
            set { this[nameof(SelectedMailChimpTemplate)] = value; }
        }

        public Send_Email_Using_MailChimp_Template_v1(ICrateManager crateManager, IMailChimpIntegration mailChimpIntegration) : base(crateManager)
        {
            _mailChimpIntegration = mailChimpIntegration;
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource EmailAddress { get; set; }

            public TextSource EmailSubject { get; set; }

            public DropDownList MailChimpTemplates { get; set; }

            [DynamicControls]
            public List<TextSource> EditableTemplateFields { get; set; }

            public ActivityUi() : this(new UiBuilder()) { }

            public ActivityUi(UiBuilder uiBuilder)
            {
                EmailAddress = uiBuilder.CreateSpecificOrUpstreamValueChooser("Email Address", nameof(EmailAddress), CrateManifestTypes.StandardDesignTimeFields, requestUpstream: true);

                EmailSubject = uiBuilder.CreateSpecificOrUpstreamValueChooser("Email Subject", nameof(EmailSubject), CrateManifestTypes.StandardDesignTimeFields, requestUpstream: true);

                MailChimpTemplates = new DropDownList
                {
                    Label = "Select a MailChimp Template",
                    Name = nameof(MailChimpTemplates),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                EditableTemplateFields = new List<TextSource>();

                Controls = new List<ControlDefinitionDTO>() { EmailAddress, EmailSubject, MailChimpTemplates };
            }

            public void ClearDynamicFields()
            {
                EditableTemplateFields?.Clear();
            }
        }

        public override async Task Initialize()
        {
            ActivityUI.MailChimpTemplates.ListItems = (await _mailChimpIntegration.GetTemplates(AuthorizationToken))
            .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
        }

        public override async Task FollowUp()
        {
            if (!string.IsNullOrEmpty(ActivityUI.MailChimpTemplates.Value))
            {
                var previousGroup = SelectedMailChimpTemplate;
                if (string.IsNullOrEmpty(previousGroup) || !string.Equals(previousGroup, ActivityUI.MailChimpTemplates.Value))
                {
                    //get all editable fields
                    var templateSections = await _mailChimpIntegration.GetTemplateSections(AuthorizationToken, ActivityUI.MailChimpTemplates.Value);
                }
                SelectedMailChimpTemplate = ActivityUI.MailChimpTemplates.Value;
                ActivityUI.MailChimpTemplates.ListItems = (await _mailChimpIntegration.GetTemplates(AuthorizationToken))
                    .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
                ActivityUI.MailChimpTemplates.Value = SelectedMailChimpTemplate;
            }
            else
            {
                ActivityUI.MailChimpTemplates.ListItems.Clear();
                ActivityUI.MailChimpTemplates.selectedKey = string.Empty;
                ActivityUI.MailChimpTemplates.Value = string.Empty;
                SelectedMailChimpTemplate = string.Empty;
            }
        }

        public override Task Run()
        {
            throw new NotImplementedException();
        }
    }
}