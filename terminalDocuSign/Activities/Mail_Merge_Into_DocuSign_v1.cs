using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Infrastructure.Behaviors;
using Fr8.TerminalBase.Infrastructure.States;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using terminalDocuSign.Activities;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Actions
{
    public class Mail_Merge_Into_DocuSign_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("ccdf8156-39fb-4082-99a4-629ec5cf1b23"),
            Name = "Mail_Merge_Into_DocuSign",
            Label = "Mail Merge Into DocuSign",
            Version = "1",
            NeedsAuthentication = true,
            MinPaneWidth = 500,
            Tags = Tags.UsesReconfigureList,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Solution }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        private string _dataSourceValue;

        private DropDownList _docuSignTemplate;

        private const string SolutionName = "Mail Merge Into DocuSign";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";

        private const string SolutionBody = @"<p>This solution is designed to take data from any table-like source (initially supported: Microsoft Excel and Google Sheets) and create and send DocuSign Envelopes. A DocuSign Template is used to generate the envelopes, and Fr8 makes it easy to map data from the sources to the DocuSign Template for automatic insertion.</p>
                                              <p>This Activity also highlights the use of the Loop activity, which can process any amount of table data, one row at a time.</p>
                                              <iframe src='https://player.vimeo.com/video/162762690' width='500' height='343' frameborder='0' webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";


        protected override string ActivityUserFriendlyName => SolutionName;


        public Mail_Merge_Into_DocuSign_v1(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager, docuSignManager)
        {
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public override Task Run()
        {
            Success();
            return Task.FromResult(0);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private async Task CreateConfigurationControlsCrate()
        {
            var controlList = new List<ControlDefinitionDTO>
            {
                new DropDownList()
                {
                Label = "1. Where is your Source Data?",
                Name = "DataSource",
                ListItems = await GetDataSourceListItems("Table Data Generator"),
                Required = true
                },
                CreateDocuSignTemplatePicker(false, "DocuSignTemplate", "2. Use which DocuSign Template?"),
                new Button()
            {
                Label = "Prepare Mail Merge",
                Name = "Continue",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
                }
            };

            AddControls(controlList);
        }

        private async Task<List<ListItem>> GetDataSourceListItems(string tag)
        {
            var curActivityTemplates = await HubCommunicator.GetActivityTemplates(tag);
            return curActivityTemplates.Select(at => new ListItem() { Key = at.Label, Value = at.Name }).ToList();
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        public override async Task Initialize()
        {
            //build a controls crate to render the pane
            await CreateConfigurationControlsCrate();
            FillDocuSignTemplateSource("DocuSignTemplate");
        }

        protected override Task Validate()
        {
            if (!IsContinueButtonClicked())
            {
                //No need to validate for now
                return Task.FromResult(0);
            }

            var templateList = GetControl<DropDownList>("DocuSignTemplate");

            if (ValidationManager.ValidateControlExistance(templateList))
            {
                ValidationManager.ValidateTemplateList(templateList);
            }

            var sourceConfigControl = GetControl<DropDownList>("DataSource");

            if (ValidationManager.ValidateControlExistance(sourceConfigControl))
            {
                if (DocuSignValidationUtils.AtLeastOneItemExists(sourceConfigControl))
                {
                    if (!DocuSignValidationUtils.ItemIsSelected(sourceConfigControl))
                    {
                        ValidationManager.SetError("Data source is not selected", sourceConfigControl);
                    }
                }
                else
                {
                    ValidationManager.SetError("No data source exists", sourceConfigControl);
                }
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        protected override ConfigurationRequestType GetConfigurationRequestType()
        {
            if (Storage == null || !Storage.Any())
            {
                return ConfigurationRequestType.Initial;
            }

            // If no values selected in textboxes, remain on initial phase
            DropDownList dataSource = GetControl<DropDownList>("DataSource");
            if (dataSource.Value != null)
                _dataSourceValue = dataSource.Value;
            _docuSignTemplate = GetControl<DropDownList>("DocuSignTemplate");
            return ConfigurationRequestType.Followup;
        }

        /// <summary>
        /// Checks if activity template generates table data
        /// TODO: find a smoother (unified) way for this
        /// </summary>
        /// <returns></returns>
        private bool DoesActivityTemplateGenerateTableData(ActivityTemplateDTO activityTemplate)
        {
            return activityTemplate.Tags != null && activityTemplate.Tags.Split(',').Any(t => t.ToLowerInvariant().Contains("table"));
        }

        private bool IsContinueButtonClicked()
        {
            Button button = GetControl<Button>("Continue");
            return button != null && button.Clicked;
        }

        public override async Task FollowUp()
        {
            //if we already have children we should return
            //or if the button was not clicked
            if (!IsContinueButtonClicked() && ActivityPayload.ChildrenActivities.Count < 1)
            {
                return;
            }

            var reconfigList = new List<ConfigurationRequest>()
            {
                new ConfigurationRequest()
                {
                    HasActivityMethod = HasFirstChildActivity,
                    CreateActivityMethod = CreateFirstChildActivity,
                    ConfigureActivityMethod = ConfigureFirstChildActivity,
                    ChildActivityIndex = 1
                },
                new ConfigurationRequest()
                {
                    HasActivityMethod = HasSecondChildActivity,
                    CreateActivityMethod = CreateSecondChildActivity,
                    ConfigureActivityMethod = ConfigureSecondChildActivity,
                    ChildActivityIndex = 2
                }
            };

            var behavior = new ReconfigurationListBehavior();
            await behavior.ReconfigureActivities(ActivityPayload, AuthorizationToken, reconfigList);
        }

        private Task<bool> HasFirstChildActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildrenActivities == null)
            {
                return Task.FromResult(false);
            }

            var activityExists = context.SolutionActivity
                .ChildrenActivities
                .OfType<ActivityPayload>()
                .Any(x => x.ActivityTemplate.Name == _dataSourceValue
                    && x.Ordering == 1
                );

            return Task.FromResult(activityExists);
        }

        /// <summary>
        /// TODO this part is ugly - why do we load those activities from hub in activity
        /// we already have a code in base class that does this operation with caching
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<ActivityPayload> CreateFirstChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null, true))
                .Select(Mapper.Map<ActivityTemplateDO>)
                .ToList();

            // Let's check if activity template generates table data
            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);

            var selectedReceiverTemplate = await HubCommunicator.GetActivityTemplate(selectedReceiver.Id);

            var dataSourceActivity = await HubCommunicator.AddAndConfigureChildActivity(
                context.SolutionActivity,
                selectedReceiverTemplate,
                order: 1
            );

            context.SolutionActivity.ChildrenActivities.Remove(dataSourceActivity);
            context.SolutionActivity.ChildrenActivities.Insert(0, dataSourceActivity);
            return dataSourceActivity;
        }

        private async Task<ActivityPayload> ConfigureFirstChildActivity(ReconfigurationContext context)
        {
            var activity = context.SolutionActivity.ChildrenActivities
                .OfType<ActivityPayload>()
                .Single(x => x.Ordering == 1);

            activity.CrateStorage = new CrateStorage();
            activity = await HubCommunicator.ConfigureChildActivity(context.SolutionActivity, activity);
            return activity;
        }

        private async Task<bool> HasSecondChildActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildrenActivities == null)
            {
                return false;
            }

            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null, true))
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityPayload parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopActivity = context.SolutionActivity.ChildrenActivities
                    .OfType<ActivityPayload>()
                    .SingleOrDefault(x => x.ActivityTemplate.Name == "Loop" && x.Ordering == 2);

                if (loopActivity == null)
                {
                    return false;
                }

                parentActivity = loopActivity;
                activityIndex = 1;
            }
            else
            {
                parentActivity = context.SolutionActivity;
                activityIndex = 2;
            }

            if (parentActivity.ChildrenActivities.Count != 1)
            {
                return false;
            }

            var sendDocuSignEnvelope = parentActivity.ChildrenActivities
                .OfType<ActivityPayload>()
                .SingleOrDefault(x => x.ActivityTemplate.Name == "Send_DocuSign_Envelope"
                    && x.Ordering == activityIndex
                );

            return (sendDocuSignEnvelope != null);
        }

        private async Task<ActivityPayload> CreateSecondChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null, true))
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityPayload parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopAT = await HubCommunicator.GetActivityTemplate("terminalFr8Core", "Loop");
                var loopActivity = await HubCommunicator.AddAndConfigureChildActivity(context.SolutionActivity, loopAT, "Loop", "Loop", 2);
                var crateChooser = ActivityConfigurator.GetControl<CrateChooser>(loopActivity, "Available_Crates");
                var firstActivity = context.SolutionActivity.ChildrenActivities.OrderBy(x => x.Ordering).First();
                var firstActivityCrates = firstActivity.CrateStorage.CrateContentsOfType<CrateDescriptionCM>().FirstOrDefault();

                crateChooser.CrateDescriptions = firstActivityCrates?.CrateDescriptions;

                var tableDescription = crateChooser.CrateDescriptions?.FirstOrDefault(c => c.ManifestId == (int)MT.StandardTableData);
                if (tableDescription != null)
                {
                    tableDescription.Selected = true;
                }

                parentActivity = loopActivity;
                activityIndex = 1;
            }
            else
            {
                parentActivity = context.SolutionActivity;
                activityIndex = 2;
            }

            var sendDocusignEnvelopeAT = await HubCommunicator.GetActivityTemplate("terminalDocuSign", "Send_DocuSign_Envelope", activityTemplateVersion: "2");
            var sendDocuSignActivity = await HubCommunicator.AddAndConfigureChildActivity(parentActivity, sendDocusignEnvelopeAT, order: activityIndex);
            // Set docusign template
            ActivityConfigurator.SetControlValue(sendDocuSignActivity, "TemplateSelector",
                _docuSignTemplate.ListItems.FirstOrDefault(a => a.Key == _docuSignTemplate.selectedKey)
            );

            await HubCommunicator.ConfigureChildActivity(parentActivity, sendDocuSignActivity);
            return activityIndex == 1 ? sendDocuSignActivity : parentActivity;
        }

        private async Task<ActivityPayload> ConfigureSecondChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null, true))
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityPayload parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopActivity = context.SolutionActivity.ChildrenActivities
                    .OfType<ActivityPayload>()
                    .SingleOrDefault(x => x.ActivityTemplate.Name == "Loop" && x.Ordering == 2);

                if (loopActivity == null)
                {
                    throw new ApplicationException("Invalid solution structure, no Loop activity found.");
                }

                loopActivity = await HubCommunicator.ConfigureChildActivity(context.SolutionActivity, loopActivity);

                var crateChooser = ActivityConfigurator.GetControl<CrateChooser>(loopActivity, "Available_Crates");
                var tableDescription = crateChooser.CrateDescriptions.FirstOrDefault(c => c.ManifestId == (int)MT.StandardTableData);
                if (tableDescription != null)
                {
                    tableDescription.Selected = true;
                }
                parentActivity = loopActivity;
                activityIndex = 1;
            }
            else
            {
                parentActivity = context.SolutionActivity;
                activityIndex = 2;
            }

            var sendDocuSignEnvelope = parentActivity.ChildrenActivities
                .OfType<ActivityPayload>()
                .Single(x => x.ActivityTemplate.Name == "Send_DocuSign_Envelope"
                    && x.Ordering == activityIndex
                );

            sendDocuSignEnvelope.CrateStorage = new CrateStorage();
            sendDocuSignEnvelope = await HubCommunicator.ConfigureChildActivity(parentActivity, sendDocuSignEnvelope);

            ActivityConfigurator.SetControlValue(
                sendDocuSignEnvelope,
                "TemplateSelector",
                _docuSignTemplate.ListItems
                    .FirstOrDefault(a => a.Key == _docuSignTemplate.selectedKey)
            );

            sendDocuSignEnvelope = await HubCommunicator.ConfigureChildActivity(parentActivity, sendDocuSignEnvelope);

            return sendDocuSignEnvelope;
        }

        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        protected override Task<DocumentationResponseDTO> GetDocumentation(string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);

            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainMailMerge"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution helps you to work with email and move data from them to DocuSign service"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(new DocumentationResponseDTO("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    new DocumentationResponseDTO("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}