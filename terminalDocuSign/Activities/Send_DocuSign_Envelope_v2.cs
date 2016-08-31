using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using terminalDocuSign.Activities;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v2 : DocuSignActivity<Send_DocuSign_Envelope_v2.ActivityUi>
    {
        private readonly IConfigRepository _configRepository;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("2b1b4d98-9eb1-4cba-8baa-a6247cd86dce"),
            Version = "2",
            Name = "Send_DocuSign_Envelope",
            Label = "Send DocuSign Envelope",
            Tags = string.Join(",", Tags.EmailDeliverer),
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList TemplateSelector { get; set; }

            [DynamicControls]
            public List<TextSource> RolesFields { get; set; }

            [DynamicControls]
            public List<TextSource> TextFields { get; set; }

            [DynamicControls]
            public List<CheckBox> CheckBoxFields { get; set; }

            [DynamicControls]
            public List<RadioButtonGroup> RadioButtonGroupFields { get; set; }

            [DynamicControls]
            public List<DropDownList> DropDownListFields { get; set; }

            public ActivityUi()
            {
                TemplateSelector = new DropDownList
                {
                    Name = nameof(TemplateSelector),
                    Label = "Use DocuSign Template",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                RolesFields = new List<TextSource>();
                TextFields = new List<TextSource>();
                CheckBoxFields = new List<CheckBox>();
                RadioButtonGroupFields = new List<RadioButtonGroup>();
                DropDownListFields = new List<DropDownList>();
                Controls.Add(TemplateSelector);
            }

            public void ClearDynamicFields()
            {
                RolesFields?.Clear();
                TextFields?.Clear();
                CheckBoxFields?.Clear();
                RadioButtonGroupFields?.Clear();
                DropDownListFields?.Clear();
            }
        }

        private const string UserFieldsAndRolesCrateLabel = "Fields and Roles";
        private const string AdvisoryName = "DocuSign Template Warning";
        private const string AdvisoryContent = "In your selected template you have fields with default values. Those can be changes inside advanced DocuSign UI to frendlier label.";

        public Send_DocuSign_Envelope_v2(ICrateManager crateManager, IDocuSignManager docuSignManager, IConfigRepository configRepository)
            : base(crateManager, docuSignManager)
        {
            _configRepository = configRepository;
            DisableValidationOnFollowup = true;
        }

        public override Task Initialize()
        {
            LoadDocuSignTemplates();
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            //Load DocuSign template again in case there are new templates available
            LoadDocuSignTemplates();
            var selectedTemplateId = SelectedTemplateId;
            //If template selection is cleared we should remove existing template fields
            if (string.IsNullOrEmpty(selectedTemplateId))
            {
                PreviousSelectedTemplateId = null;
                ActivityUI.ClearDynamicFields();
                Storage.RemoveByLabel(UserFieldsAndRolesCrateLabel);
                return;
            }
            if (selectedTemplateId == PreviousSelectedTemplateId)
            {
                return;
            }
            ActivityUI.ClearDynamicFields();
            PreviousSelectedTemplateId = selectedTemplateId;
            var docuSignConfiguration = DocuSignManager.SetUp(AuthorizationToken);
            var tabsAndFields = DocuSignManager.GetTemplateRecipientsTabsAndDocuSignTabs(docuSignConfiguration, selectedTemplateId);
            var roles = tabsAndFields.Item1.Where(x => x.Tags.Contains(DocuSignConstants.DocuSignSignerTag, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            var userDefinedFields = tabsAndFields.Item1.Where(x => x.Tags.Contains(DocuSignConstants.DocuSignTabTag));
            var envelopeData = tabsAndFields.Item2.ToLookup(x => x.Fr8DisplayType);

            //check for DocuSign default template names and add advisory json
            var hasDefaultNames = DocuSignManager.DocuSignTemplateDefaultNames(tabsAndFields.Item2);
            if (hasDefaultNames)
            {
                var advisoryCrate = Storage.CratesOfType<AdvisoryMessagesCM>().FirstOrDefault();
                var currentAdvisoryResults = advisoryCrate == null ? new AdvisoryMessagesCM() : advisoryCrate.Content;

                var advisory = currentAdvisoryResults.Advisories.FirstOrDefault(x => x.Name == AdvisoryName);

                if (advisory == null)
                {
                    currentAdvisoryResults.Advisories.Add(new AdvisoryMessageDTO { Name = AdvisoryName, Content = AdvisoryContent });
                }
                else
                {
                    advisory.Content = AdvisoryContent;
                }

                Storage.Add(Crate.FromContent("Advisories", currentAdvisoryResults));
            }
            //Add TextSource control for every DocuSign role to activity UI
            ActivityUI.RolesFields.AddRange(roles.Select(x => UiBuilder.CreateSpecificOrUpstreamValueChooser(x.Key, x.Key, requestUpstream: true, specificValue: x.Value)));
            //Add TextSrouce control for every DocuSign template text field to activity UI
            ActivityUI.TextFields.AddRange(envelopeData[ControlTypes.TextBox].Select(x => UiBuilder.CreateSpecificOrUpstreamValueChooser(x.Name, x.Name, requestUpstream: true, specificValue: x.Value)));
            //Add RadioButtonGroup with respective options for every DocuSign template radio selection field to activity UI
            ActivityUI.RadioButtonGroupFields.AddRange(
                envelopeData[ControlTypes.RadioButtonGroup]
                .OfType<DocuSignMultipleOptionsTabDTO>()
                .Select(x => new RadioButtonGroup
                {
                    GroupName = x.Name,
                    Name = x.Name,
                    Label = $"For the <strong>{x.Name}</strong>, use:",
                    Radios = x.Items.Select(y => new RadioButtonOption
                    {
                        Name = y.Value,
                        Value = y.Value,
                        Selected = y.Selected
                    })
                    .ToList()
                }));
            //Add CheckBox for every DocuSign template yes/no field to activity UI
            ActivityUI.CheckBoxFields.AddRange(envelopeData[ControlTypes.CheckBox].Select(x => new CheckBox { Name = x.Name, Label = x.Name, Selected = Convert.ToBoolean(x.Value) }));
            //Add DropDownList for every DocuSign template list selection field to activity UI
            ActivityUI.DropDownListFields.AddRange(
                envelopeData[ControlTypes.DropDownList]
                    .OfType<DocuSignMultipleOptionsTabDTO>()
                    .Select(x => new DropDownList
                    {
                        Name = x.Name,
                        Label = $"For the <strong>{x.Name}</strong>, use:",
                        ListItems = x.Items.Select(y => new ListItem
                        {
                            Key = string.IsNullOrEmpty(y.Value) ? y.Text : y.Value,
                            Value = string.IsNullOrEmpty(y.Text) ? y.Value : y.Text,
                            Selected = y.Selected
                        })
                        .ToList()
                    }));

            Storage.ReplaceByLabel(Crate.FromContent(UserFieldsAndRolesCrateLabel, new KeyValueListCM(userDefinedFields.Concat(roles))));
        }

        protected override Task Validate()
        {
            if (string.IsNullOrEmpty(SelectedTemplateId))
            {
                ValidationManager.SetError("Template was not selected", ActivityUI.TemplateSelector);
            }

            foreach (var roleControl in ActivityUI.RolesFields.Where(x => x.InitialLabel.Contains(DocuSignConstants.DocuSignRoleEmail)))
            {
                ValidationManager.ValidateEmail(_configRepository, roleControl);
            }

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var userDefinedFields = Storage.FirstCrateOrDefault<KeyValueListCM>(x => x.Label == UserFieldsAndRolesCrateLabel);
            if (userDefinedFields == null)
            {
                throw new ActivityExecutionException("Activity storage doesn't contain info about DocuSign envelope properties. This may indicate that activity was not properly configured. Try to reconfigure this activity");
            }
            var allFields = userDefinedFields.Content.Values;
            var roleValues = ActivityUI.RolesFields.Select(x => new { x.Name, Value = x.TextValue }).ToDictionary(x => x.Name, x => x.Value);
            var fieldValues = ActivityUI.CheckBoxFields.Select(x => new { x.Name, Value = x.Selected.ToString().ToLower() })
                                                   .Concat(ActivityUI.DropDownListFields.Select(x => new { x.Name, Value = x.selectedKey }))
                                                   .Concat(ActivityUI.RadioButtonGroupFields.Select(x => new { x.Name, x.Radios.FirstOrDefault(y => y.Selected)?.Value }))
                                                   .Concat(ActivityUI.TextFields.Select(x => new { x.Name, Value = x.TextValue }))
                                                   .ToDictionary(x => x.Name, x => x.Value);
            var docuSignConfiguration = DocuSignManager.SetUp(AuthorizationToken);
            var roleFields = allFields.Where(x => x.Tags.Contains(DocuSignConstants.DocuSignSignerTag, StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var roleField in roleFields)
            {
                roleField.Value = roleValues[roleField.Key];
            }
            var userFields = allFields.Where(x => x.Tags.Contains(DocuSignConstants.DocuSignTabTag)).ToList();
            foreach (var userField in userFields)
            {
                userField.Value = fieldValues[userField.Key];
            }
            DocuSignManager.SendAnEnvelopeFromTemplate(docuSignConfiguration, roleFields, userFields, SelectedTemplateId);
        }

        #region Implementations details

        private void LoadDocuSignTemplates()
        {
            var selectedTemplateId = SelectedTemplateId;
            var configuratin = DocuSignManager.SetUp(AuthorizationToken);
            ActivityUI.TemplateSelector.ListItems = DocuSignManager.GetTemplatesList(configuratin)
                                                                              .Select(x => new ListItem { Key = x.Key, Value = x.Value })
                                                                              .ToList();
            if (string.IsNullOrEmpty(selectedTemplateId))
            {
                return;
            }
            ActivityUI.TemplateSelector.SelectByValue(selectedTemplateId);
        }

        private string PreviousSelectedTemplateId
        {
            get { return this[nameof(PreviousSelectedTemplateId)]; }
            set { this[nameof(PreviousSelectedTemplateId)] = value; }
        }

        private string SelectedTemplateId => ActivityUI.TemplateSelector.Value;

        #endregion
    }
}