using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.Manifests;
using Fr8Data.States;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services.New_Api;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Infrastructure.Behaviors;
using Utilities;

namespace terminalDocuSign.Activities
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
        }

        //TODO: remove this constructor after introducing constructor injection
        public Send_DocuSign_Envelope_v2() : this(ObjectFactory.GetInstance<IDocuSignManager>())
        {
        }

        public Send_DocuSign_Envelope_v2(IDocuSignManager docuSignManager) : base(docuSignManager)
        {
        }

        protected override Task Initialize(CrateSignaller crateSignaller)
        {
            var configuratin = DocuSignManager.SetUp(AuthorizationToken);
            ConfigurationControls.TemplateSelector.ListItems = DocuSignManager.GetTemplatesList(configuratin)
                                                                              .Select(x => new ListItem { Key = x.Key, Value = x.Value })
                                                                              .ToList();
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
                ClearTemplateFields();
                return;
            }
            if (selectedTemplateId == PreviousSelectedTemplateId)
            {
                return;
            }
            PreviousSelectedTemplateId = selectedTemplateId;
            var docuSignConfiguration = DocuSignManager.SetUp(AuthorizationToken);
            var tabsAndFields = DocuSignManager.GetTemplateRecipientsTabsAndDocuSignTabs(docuSignConfiguration, selectedTemplateId);
            var roles = tabsAndFields.Item1.Where(x => x.Tags.Contains("DocuSigner", StringComparison.InvariantCultureIgnoreCase)).ToArray();
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
            ConfigurationControls.CheckBoxFields.AddRange(envelopeData[ControlTypes.TextBox].Select(x => new CheckBox { Name = x.Name, Label = x.Name }));
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
        }

        protected override Task Validate(ValidationManager validationManager)
        {
            return base.Validate(validationManager);
        }

        protected override Task RunCurrentActivity()
        {
            throw new NotImplementedException();
        }

        #region Implementations details
        private void ClearTemplateFields()
        {
            ConfigurationControls.CheckBoxFields.Clear();
            ConfigurationControls.DropDownListFields.Clear();
            ConfigurationControls.RadioButtonGroupFields.Clear();
            ConfigurationControls.TextFields.Clear();
        }

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