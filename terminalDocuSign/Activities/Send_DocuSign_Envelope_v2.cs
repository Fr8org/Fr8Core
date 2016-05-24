using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Validations;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Fr8Data.States;
using StructureMap;
using terminalDocuSign.Activities;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalDocuSign.Actions
{
    public class Send_DocuSign_Envelope_v2 : EnhancedDocuSignActivity<Send_DocuSign_Envelope_v2.ActivityUi>
    {
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

        //TODO: remove this constructor after introducing constructor injection
        public Send_DocuSign_Envelope_v2() : this(ObjectFactory.GetInstance<IDocuSignManager>())
        {
        }

        public Send_DocuSign_Envelope_v2(IDocuSignManager docuSignManager) : base(docuSignManager)
        {
            DisableValidationOnFollowup = true;
        }

        protected override Task Initialize(CrateSignaller crateSignaller)
        {
            LoadDocuSignTemplates();
            return Task.FromResult(0);
        }

        protected override async Task Configure(CrateSignaller crateSignaller, ValidationManager validationManager)
        {
            //Load DocuSign template again in case there are new templates available
            LoadDocuSignTemplates();
            var selectedTemplateId = SelectedTemplateId;
            //If template selection is cleared we should remove existing template fields
            if (string.IsNullOrEmpty(selectedTemplateId))
            {
                PreviousSelectedTemplateId = null;
                ConfigurationControls.ClearDynamicFields();
                CurrentActivityStorage.RemoveByLabel(UserFieldsAndRolesCrateLabel);
                return;
            }
            if (selectedTemplateId == PreviousSelectedTemplateId)
            {
                return;
            }
            ConfigurationControls.ClearDynamicFields();
            PreviousSelectedTemplateId = selectedTemplateId;
            var docuSignConfiguration = DocuSignManager.SetUp(AuthorizationToken);
            var tabsAndFields = DocuSignManager.GetTemplateRecipientsTabsAndDocuSignTabs(docuSignConfiguration, selectedTemplateId);
            var roles = tabsAndFields.Item1.Where(x => x.Tags.Contains(DocuSignConstants.DocuSignSignerTag, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            var userDefinedFields = tabsAndFields.Item1.Where(x => x.Tags.Contains(DocuSignConstants.DocuSignTabTag));
            var envelopeData = tabsAndFields.Item2.ToLookup(x => x.Fr8DisplayType);
            //Add TextSource control for every DocuSign role to activity UI
            ConfigurationControls.RolesFields.AddRange(roles.Select(x => UiBuilder.CreateSpecificOrUpstreamValueChooser(x.Key, x.Key, requestUpstream: true)));
            //Add TextSrouce control for every DocuSign template text field to activity UI
            ConfigurationControls.TextFields.AddRange(envelopeData[ControlTypes.TextBox].Select(x => UiBuilder.CreateSpecificOrUpstreamValueChooser(x.Name, x.Name, requestUpstream: true)));
            //Add RadioButtonGroup with respective options for every DocuSign template radio selection field to activity UI
            ConfigurationControls.RadioButtonGroupFields.AddRange(
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
            ConfigurationControls.CheckBoxFields.AddRange(envelopeData[ControlTypes.CheckBox].Select(x => new CheckBox { Name = x.Name, Label = x.Name }));
            //Add DropDownList for every DocuSign template list selection field to activity UI
            ConfigurationControls.DropDownListFields.AddRange(
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

            CurrentActivityStorage.ReplaceByLabel(Crate.FromContent(UserFieldsAndRolesCrateLabel, new FieldDescriptionsCM(userDefinedFields.Concat(roles)), AvailabilityType.Configuration));
        }

        protected override ValidationManager CreateValidationManager()
        {
            var validationResult = GetOrCreateValidationResults();
            return new EnhancedValidationManager<ActivityUi>(validationResult, this,  TryGetCurrentPayloadStorage());
        }

        protected override Task Validate(ValidationManager validationManager)
        {
            if (string.IsNullOrEmpty(SelectedTemplateId))
            {
                validationManager.SetError("Template was not selected", ConfigurationControls.TemplateSelector);
            }
            foreach (var roleControl in ConfigurationControls.RolesFields.Where(x => x.InitialLabel.Contains(DocuSignConstants.DocuSignRoleEmail)))
            {
                validationManager.ValidateEmail(roleControl);
            }
            return Task.FromResult(0);
        }

        protected override async Task RunCurrentActivity()
        {
            var userDefinedFields = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == UserFieldsAndRolesCrateLabel);
            if (userDefinedFields == null)
            {
                throw new ActivityExecutionException();
            }
            var allFields = userDefinedFields.Content.Fields;
            var roleValues = ConfigurationControls.RolesFields.Select(x => new { x.Name, Value = x.GetValue(CurrentPayloadStorage) }).ToDictionary(x => x.Name, x => x.Value);
            var fieldValues = ConfigurationControls.CheckBoxFields.Select(x => new { x.Name, Value = x.Selected.ToString().ToLower() })
                                                   .Concat(ConfigurationControls.DropDownListFields.Select(x => new { x.Name, Value = x.selectedKey }))
                                                   .Concat(ConfigurationControls.RadioButtonGroupFields.Select(x => new { x.Name, x.Radios.FirstOrDefault(y => y.Selected)?.Value }))
                                                   .Concat(ConfigurationControls.TextFields.Select(x => new { x.Name, Value = x.GetValue(CurrentPayloadStorage) }))
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
            ConfigurationControls.TemplateSelector.ListItems = DocuSignManager.GetTemplatesList(configuratin)
                                                                              .Select(x => new ListItem { Key = x.Key, Value = x.Value })
                                                                              .ToList();
            if (string.IsNullOrEmpty(selectedTemplateId))
            {
                return;
            }
            ConfigurationControls.TemplateSelector.SelectByValue(selectedTemplateId);
        }

        private string PreviousSelectedTemplateId
        {
            get { return this[nameof(PreviousSelectedTemplateId)]; }
            set { this[nameof(PreviousSelectedTemplateId)] = value; }
        }

        private string SelectedTemplateId => ConfigurationControls.TemplateSelector.Value;

        #endregion
    }
}