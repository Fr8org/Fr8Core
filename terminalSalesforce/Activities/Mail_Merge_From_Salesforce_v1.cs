using Data.Control;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.Linq;
using TerminalBase.BaseClasses;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using Data.Crates;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Services;
using TerminalBase.Infrastructure.Behaviors;
using Data.Entities;
using AutoMapper;
using Data.Interfaces;
using Utilities.Configuration.Azure;
using Hub.Managers;
using Data.Constants;
using ServiceStack;

namespace terminalSalesforce.Actions
{
    public class Mail_Merge_From_Salesforce_v1 : BaseSalesforceTerminalActivity<Mail_Merge_From_Salesforce_v1.ActivityUi>
    {

        private const string SolutionName = "Mail Merge From Salesforce";
        private const double SolutionVersion = 1.0;
        private const string SolutionBody = @"<p>Pull data from a variety of sources, including Excel files, 
                                            Google Sheets, and databases, and merge the data into your Salesforce template. 
                                            You can link specific fields from your source data to Salesforce fields</p>";
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
                        ManifestType = CrateManifestTypes.StandardQueryFields
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

        public Mail_Merge_From_Salesforce_v1()
        {
            ActivityName = "Mail Merge from Salesforce";
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
        }

        protected override Task<bool> Validate()
        {
            if (ConfigurationControls.RunMailMergeButton.Clicked)
            {
                ConfigurationControls.SalesforceObjectSelector.ErrorMessage = string.IsNullOrEmpty(ConfigurationControls.SalesforceObjectSelector.selectedKey)
                                                                          ? "Object is not selected"
                                                                          : string.Empty;
                ConfigurationControls.MailSenderActivitySelector.ErrorMessage = string.IsNullOrEmpty(ConfigurationControls.MailSenderActivitySelector.selectedKey)
                                                                                ? "Mail sender is not selected"
                                                                                : string.Empty;
                var isValid = string.IsNullOrEmpty(ConfigurationControls.SalesforceObjectSelector.ErrorMessage)
                           && string.IsNullOrEmpty(ConfigurationControls.MailSenderActivitySelector.ErrorMessage);
                if (!isValid)
                {
                    ConfigurationControls.RunMailMergeButton.Clicked = false;
                }
                return Task.FromResult(isValid);
            }
            return Task.FromResult(true);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            if (ConfigurationControls.RunMailMergeButton.Clicked)
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
            var behavior = new ReconfigurationListBehavior(this);
            await behavior.ReconfigureActivities(CurrentActivity, AuthorizationToken, reconfigurationList);
        }

        private async Task<bool> HasProcessingActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildNodes == null)
            {
                return false;
            }
            var loopActivity = context.SolutionActivity
                                      .ChildNodes
                                      .OfType<ActivityDO>()
                                      .SingleOrDefault(x => x.ActivityTemplate.Name == "Loop" && x.Ordering == 2);
            if (loopActivity == null || loopActivity.ChildNodes.Count != 1)
            {
                return false;
            }
            var emailSenderActivity = loopActivity.ChildNodes.OfType<ActivityDO>().SingleOrDefault();
            if (emailSenderActivity == null)
            {
                return false;
            }
            if (emailSenderActivity.ActivityTemplate.Name != ConfigurationControls.MailSenderActivitySelector.selectedKey)
            {
                return false;
            }
            return true;
        }

        private async Task<ActivityDO> CreateProcessingActivity(ReconfigurationContext context)
        {
            var loopActivity = await AddAndConfigureChildActivity(context.SolutionActivity, await GetActivityTemplate("terminalFr8Core", "Loop"), "Loop", "Loop", 2);
            using (var crateStorage = CrateManager.GetUpdatableStorage(loopActivity))
            {
                var loopConfigControls = GetConfigurationControls(crateStorage);
                var crateChooser = loopConfigControls.Controls.OfType<CrateChooser>().Single();
                var tableDescription = crateChooser.CrateDescriptions.FirstOrDefault(x => x.ManifestId == (int)MT.StandardTableData);
                if (tableDescription != null)
                {
                    tableDescription.Selected = true;
                }
            }
            var solutionActivityUi = new ActivityUi().ClonePropertiesFrom(CrateManager.GetStorage(context.SolutionActivity).FirstCrate<StandardConfigurationControlsCM>().Content) as ActivityUi;
            var mailSenderActivityTemplate = await GetActivityTemplate(Guid.Parse(solutionActivityUi.MailSenderActivitySelector.Value));
            var sendEmailActivity = await AddAndConfigureChildActivity(loopActivity, mailSenderActivityTemplate, order: 1);
            return loopActivity;
        }

        private Task<ActivityDO> ConfigureProcessingActivity(ReconfigurationContext context)
        {
            //No extra config required
            return Task.FromResult(context.SolutionActivity.ChildNodes.OfType<ActivityDO>().FirstOrDefault(x => x.Ordering == 2));
        }

        private Task<bool> HasSalesforceDataActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildNodes == null)
            {
                return Task.FromResult(false);
            }
            var result = context.SolutionActivity
                                .ChildNodes
                                .OfType<ActivityDO>()
                                .Any(x => x.ActivityTemplate.Name == "Get_Data" && x.Ordering == 2);
            return Task.FromResult(result);
        }

        private async Task<ActivityDO> CreateSalesforceDataActivity(ReconfigurationContext context)
        {
            var getSalesforceDataActivityTemplate = (await HubCommunicator.GetActivityTemplates(null))
                .First(x => x.Name == "Get_Data" && x.Terminal.Name == TerminalName && x.Version == "1");
            var dataSourceActivity = await AddAndConfigureChildActivity(
                context.SolutionActivity,
                getSalesforceDataActivityTemplate,
                order: 1);
            //This config call will make SF Get_Data activity to load properties of the selected object (and removes filter)
            CopySolutionUiValuesToSalesforceActivity(context.SolutionActivity, dataSourceActivity);
            dataSourceActivity = await ConfigureChildActivity(context.SolutionActivity, dataSourceActivity);
            //This config call will set the proper filter value for the selected object 
            CopySolutionUiValuesToSalesforceActivity(context.SolutionActivity, dataSourceActivity);
            return await ConfigureChildActivity(context.SolutionActivity, dataSourceActivity);
        }

        private async Task<ActivityDO> ConfigureSalesforceDataActivity(ReconfigurationContext context)
        {
            return context.SolutionActivity.ChildNodes.OfType<ActivityDO>().Single(x => x.Ordering == 2);
        }

        private void CopySolutionUiValuesToSalesforceActivity(ActivityDO solutionActivity, ActivityDO salesforceActivity)
        {
            using (var storage = CrateManager.GetUpdatableStorage(salesforceActivity))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new Get_Data_v1.ActivityUi().ClonePropertiesFrom(controlsCrate.Content) as Get_Data_v1.ActivityUi;
                var solutionActivityUi = new ActivityUi().ClonePropertiesFrom(CrateManager.GetStorage(solutionActivity).FirstCrate<StandardConfigurationControlsCM>().Content) as ActivityUi;
                activityUi.SalesforceObjectSelector.selectedKey = solutionActivityUi.SalesforceObjectSelector.selectedKey;
                activityUi.SalesforceObjectSelector.Value = solutionActivityUi.SalesforceObjectSelector.Value;
                activityUi.SalesforceObjectFilter.Value = solutionActivityUi.SalesforceObjectFilter.Value;
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }
        }

        private async Task ConfigureSolutionActivityUi()
        {
            var selectedObject = ConfigurationControls.SalesforceObjectSelector.selectedKey;
            if (string.IsNullOrEmpty(selectedObject))
            {
                CurrentActivityStorage.RemoveByLabel(QueryFilterCrateLabel);
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
            var queryFilterCrate = Crate<TypedFieldsCM>.FromContent(
                QueryFilterCrateLabel,
                new TypedFieldsCM(selectedObjectProperties.OrderBy(x => x.Key)
                                                                  .Select(x => new TypedFieldDTO(x.Key, x.Value, FieldType.String, new TextBox { Name = x.Key }))),
                AvailabilityType.Configuration);
            CurrentActivityStorage.ReplaceByLabel(queryFilterCrate);
            this[nameof(ActivityUi.SalesforceObjectSelector)] = selectedObject;
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.SalesforceObjectSelector.ListItems = _salesforceManager.GetSalesforceObjectTypes().Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            var activityTemplates = await HubCommunicator.GetActivityTemplates(ActivityTemplate.EmailDelivererTag, CurrentFr8UserId);
            activityTemplates.Sort((x, y) => x.Name.CompareTo(y.Name));
            ConfigurationControls.MailSenderActivitySelector.ListItems = activityTemplates
                                                                            .Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() })
                                                                            .ToList();
        }

        protected override Task RunCurrentActivity()
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
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);

            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainMailMerge"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonse(@"This solution helps you to work with email and move data from them to DocuSign service"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonse(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonse("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonse("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}