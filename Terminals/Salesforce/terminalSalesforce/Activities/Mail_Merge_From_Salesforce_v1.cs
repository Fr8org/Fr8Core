using System;
using System.Collections.Generic;
using System.Linq;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Infrastructure.Behaviors;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using ServiceStack;

namespace terminalSalesforce.Actions
{
    public class Mail_Merge_From_Salesforce_v1 : BaseSalesforceTerminalActivity<Mail_Merge_From_Salesforce_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("81c02e05-6561-4e3e-ab10-e327a5c601e9"),
            Version = "1",
            Name = "Mail_Merge_From_Salesforce",
            Label = "Mail Merge from Salesforce",
            NeedsAuthentication = true,
            MinPaneWidth = 500,
            Tags = Tags.UsesReconfigureList,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Solution }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string SolutionName = "Mail Merge From Salesforce";
        private const double SolutionVersion = 1.0;
        private const string SolutionBody = @"<p>Pull data from a variety of sources, including Excel files, 
                                            Google Sheets, and databases, and merge the data into your Salesforce template. 
                                            You can link specific fields from your source data to Salesforce fields</p>";
        private const string TerminalDisplayName = "Salesforce";
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SalesforceObjectSelector { get; set; }

            public QueryBuilder SalesforceObjectFilter { get; set; }

            public DropDownList MailSenderActivitySelector { get; set; }

            public Button RunMailMergeButton { get; set; }

            public ActivityUi()
            {
                SalesforceObjectSelector = new DropDownList
                {
                    Name = nameof(SalesforceObjectSelector),
                    Label = "Get all objects of type:",
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                SalesforceObjectFilter = new QueryBuilder
                {
                    Name = nameof(SalesforceObjectFilter),
                    Label = "That meet the following conditions:",
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        Label = QueryFilterCrateLabel,
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                };
                MailSenderActivitySelector = new DropDownList
                {
                    Name = nameof(MailSenderActivitySelector),
                    Label = "Mail Merge to",
                    Required = true
                };
                RunMailMergeButton = new Button
                {
                    Name = nameof(RunMailMergeButton),
                    Label = "Prepare Mail Merge",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfigOnClick }
                };
                Controls.Add(SalesforceObjectSelector);
                Controls.Add(SalesforceObjectFilter);
                Controls.Add(MailSenderActivitySelector);
                Controls.Add(RunMailMergeButton);
            }
        }
        //NOTE: this label must be the same as the one expected in QueryBuilder.ts
        private const string QueryFilterCrateLabel = "Queryable Criteria";

        private const string TerminalName = "terminalSalesforce";

        private readonly ISalesforceManager _salesforceManager;

        public Mail_Merge_From_Salesforce_v1(ICrateManager crateManager, ISalesforceManager salesforceManager)
            : base(crateManager)
        {
            _salesforceManager = salesforceManager;
        }

        protected override Task Validate()
        {
            if (ActivityUI.RunMailMergeButton.Clicked)
            {
                if (string.IsNullOrEmpty(ActivityUI.SalesforceObjectSelector.selectedKey))
                {
                    ValidationManager.SetError("Object is not selected", ActivityUI.SalesforceObjectSelector);
                    ActivityUI.RunMailMergeButton.Clicked = false;
                }

                if (string.IsNullOrEmpty(ActivityUI.MailSenderActivitySelector.selectedKey))
                {
                    ValidationManager.SetError("Mail sender is not selected", ActivityUI.MailSenderActivitySelector);
                    ActivityUI.RunMailMergeButton.Clicked = false;
                }
            }

            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            if (ActivityUI.RunMailMergeButton.Clicked)
            {
                await ConfigureChildActivities();
            }
            else
            {
                await ConfigureSolutionActivityUi();
            }
        }

        private async Task ConfigureChildActivities()
        {
            var reconfigurationList = new List<ConfigurationRequest>()
            {
                new ConfigurationRequest()
                {
                    HasActivityMethod = HasSalesforceDataActivity,
                    CreateActivityMethod = CreateSalesforceDataActivity,
                    ConfigureActivityMethod = ConfigureSalesforceDataActivity,
                    ChildActivityIndex = 1
                },
                new ConfigurationRequest()
                {
                    HasActivityMethod = HasProcessingActivity,
                    CreateActivityMethod = CreateProcessingActivity,
                    ConfigureActivityMethod = ConfigureProcessingActivity,
                    ChildActivityIndex = 2
                }
            };
            var behavior = new ReconfigurationListBehavior();
            await behavior.ReconfigureActivities(ActivityPayload, AuthorizationToken, reconfigurationList);
        }

        private async Task<bool> HasProcessingActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildrenActivities == null)
            {
                return false;
            }
            var loopActivity = context.SolutionActivity
                                      .ChildrenActivities
                                      .OfType<ActivityPayload>()
                                      .SingleOrDefault(x => x.ActivityTemplate.Name == "Loop" && x.Ordering == 2);
            if (loopActivity == null || loopActivity.ChildrenActivities.Count != 1)
            {
                return false;
            }
            var emailSenderActivity = loopActivity.ChildrenActivities.OfType<ActivityPayload>().SingleOrDefault();
            if (emailSenderActivity == null)
            {
                return false;
            }
            if (emailSenderActivity.ActivityTemplate.Name != ActivityUI.MailSenderActivitySelector.selectedKey)
            {
                return false;
            }
            return true;
        }

        private async Task<ActivityPayload> CreateProcessingActivity(ReconfigurationContext context)
        {
            var loopActivity = await HubCommunicator.AddAndConfigureChildActivity(context.SolutionActivity, await HubCommunicator.GetActivityTemplate("terminalFr8Core", "Loop"), "Loop", "Loop", 2);
            var loopConfigControls = ActivityConfigurator.GetConfigurationControls(loopActivity);
            var crateChooser = loopConfigControls.Controls.OfType<CrateChooser>().Single();
            var firstActivity = context.SolutionActivity.ChildrenActivities.OrderBy(x => x.Ordering).First();
            var firstActivityCrates = firstActivity.CrateStorage.CrateContentsOfType<CrateDescriptionCM>().FirstOrDefault();

            crateChooser.CrateDescriptions = firstActivityCrates?.CrateDescriptions;

            var tableDescription = crateChooser.CrateDescriptions?.FirstOrDefault(x => x.ManifestId == (int)MT.StandardTableData);
            if (tableDescription != null)
            {
                tableDescription.Selected = true;
            }

            var solutionActivityUi = new ActivityUi().ClonePropertiesFrom(context.SolutionActivity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>().Content) as ActivityUi;
            var mailSenderActivityTemplate = await HubCommunicator.GetActivityTemplate(Guid.Parse(solutionActivityUi.MailSenderActivitySelector.Value));
            var sendEmailActivity = await HubCommunicator.AddAndConfigureChildActivity(loopActivity, mailSenderActivityTemplate, order: 1);
            return loopActivity;
        }

        private Task<ActivityPayload> ConfigureProcessingActivity(ReconfigurationContext context)
        {
            //No extra config required
            return Task.FromResult(context.SolutionActivity.ChildrenActivities.OfType<ActivityPayload>().FirstOrDefault(x => x.Ordering == 2));
        }

        private Task<bool> HasSalesforceDataActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildrenActivities == null)
            {
                return Task.FromResult(false);
            }
            var result = context.SolutionActivity
                                .ChildrenActivities
                                .OfType<ActivityPayload>()
                                .Any(x => x.ActivityTemplate.Name == "Get_Data" && x.Ordering == 2);
            return Task.FromResult(result);
        }

        private async Task<ActivityPayload> CreateSalesforceDataActivity(ReconfigurationContext context)
        {
            var getSalesforceDataActivityTemplate = (await HubCommunicator.GetActivityTemplates(null))
                .First(x => x.Name == "Get_Data" && x.Terminal.Name == TerminalName && x.Version == "1");
            var dataSourceActivity = await HubCommunicator.AddAndConfigureChildActivity(
                context.SolutionActivity,
                getSalesforceDataActivityTemplate,
                order: 1);
            if (dataSourceActivity.CrateStorage.Where(a => a.ManifestType.Id == (int)MT.StandardAuthentication).FirstOrDefault() != null)
            {
                await HubCommunicator.ApplyNewToken(dataSourceActivity.Id, Guid.Parse(AuthorizationToken.Id));
                dataSourceActivity = await HubCommunicator.ConfigureActivity(dataSourceActivity);
            }

            //This config call will make SF Get_Data activity to load properties of the selected object (and removes filter)
            CopySolutionUiValuesToSalesforceActivity(context.SolutionActivity, dataSourceActivity);
            dataSourceActivity = await HubCommunicator.ConfigureChildActivity(context.SolutionActivity, dataSourceActivity);
            //This config call will set the proper filter value for the selected object 
            CopySolutionUiValuesToSalesforceActivity(context.SolutionActivity, dataSourceActivity);
            return await HubCommunicator.ConfigureChildActivity(context.SolutionActivity, dataSourceActivity);
        }

        private async Task<ActivityPayload> ConfigureSalesforceDataActivity(ReconfigurationContext context)
        {
            return context.SolutionActivity.ChildrenActivities.OfType<ActivityPayload>().Single(x => x.Ordering == 2);
        }

        private void CopySolutionUiValuesToSalesforceActivity(ActivityPayload solutionActivity, ActivityPayload salesforceActivity)
        {
            var storage = salesforceActivity.CrateStorage;
            var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
            var activityUi = new Get_Data_v1.ActivityUi().ClonePropertiesFrom(controlsCrate.Content) as Get_Data_v1.ActivityUi;
            var solutionActivityUi = new ActivityUi().ClonePropertiesFrom(solutionActivity.CrateStorage.FirstCrate<StandardConfigurationControlsCM>().Content) as ActivityUi;
            activityUi.SalesforceObjectSelector.selectedKey = solutionActivityUi.SalesforceObjectSelector.selectedKey;
            activityUi.SalesforceObjectSelector.Value = solutionActivityUi.SalesforceObjectSelector.Value;
            activityUi.SalesforceObjectFilter.Value = solutionActivityUi.SalesforceObjectFilter.Value;
            storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray())));
        }

        private async Task ConfigureSolutionActivityUi()
        {
            var selectedObject = ActivityUI.SalesforceObjectSelector.selectedKey;
            if (string.IsNullOrEmpty(selectedObject))
            {
                Storage.RemoveByLabel(QueryFilterCrateLabel);
                this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
                return;
            }
            //If the same object is selected we shouldn't do anything
            if (selectedObject == this[nameof(ActivityUi.SalesforceObjectSelector)])
            {
                return;
            }
            //Prepare new query filters from selected object properties
            var selectedObjectProperties = await _salesforceManager.GetProperties(selectedObject.ToEnum<SalesforceObjectType>(), AuthorizationToken);
            var queryFilterCrate = Crate<FieldDescriptionsCM>.FromContent(
                QueryFilterCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties)
            );

            Storage.ReplaceByLabel(queryFilterCrate);
            this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
        }

        public override async Task Initialize()
        {
            ActivityUI.SalesforceObjectSelector.ListItems = _salesforceManager.GetSalesforceObjectTypes().Select(x => new ListItem { Key = x.Name, Value = x.Label }).ToList();
            var activityTemplates = await HubCommunicator.GetActivityTemplates(Tags.EmailDeliverer, true);
            activityTemplates.Sort((x, y) => x.Name.CompareTo(y.Name));
            ActivityUI.MailSenderActivitySelector.ListItems = activityTemplates
                                                                            .Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() })
                                                                            .ToList();
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        protected override Task<DocumentationResponseDTO> GetDocumentation(string documentationType)
        {
            if (documentationType.Contains("MainPage"))
            {
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalDisplayName, SolutionBody);
                return Task.FromResult(curSolutionPage);

            }
            if (documentationType.Contains("HelpMenu"))
            {
                if (documentationType.Contains("ExplainMailMerge"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution helps you to work with email and move data from them to DocuSign service"));
                }
                if (documentationType.Contains("ExplainService"))
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