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

namespace terminalSalesforce.Actions
{
    public class MailMergeFromSalesforce_v1 : EnhancedTerminalActivity<MailMergeFromSalesforce_v1.ActivityUi>
    {
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
                    Label = "Get Which Object?",
                    Required = true,
                    Events = new List<ControlEvent> {  ControlEvent.RequestConfig }
                };
                SalesforceObjectFilter = new QueryBuilder
                {
                    Name = nameof(SalesforceObjectFilter),
                    Label = "Meeting Which Conditions?",
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

        private readonly ISalesforceManager _salesforceManager;

        private readonly int terminalId;

        public MailMergeFromSalesforce_v1() : base(true)
        {
            ActivityName = "Mail Merge from Salesforce";
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
            var terminalName = CloudConfigurationManager.GetSetting("TerminalName");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalId = uow.TerminalRepository
                                .GetQuery()
                                .Where(x => x.Name == terminalName)
                                .Select(x => x.Id)
                                .First();
            }
        }

        protected override Task<bool> Validate()
        {
            if (!ConfigurationControls.RunMailMergeButton.Clicked)
            {
                return Task.FromResult(true);
            }
            //We perform validation only when user clicks on 'Prepare Mail Merge' button so he is not bothered with error messages untill he makes final step
            ConfigurationControls.SalesforceObjectSelector.ErrorMessage = string.IsNullOrEmpty(ConfigurationControls.SalesforceObjectSelector.selectedKey)
                                                                            ? "Object is not selected"
                                                                            : string.Empty;
            ConfigurationControls.MailSenderActivitySelector.ErrorMessage = string.IsNullOrEmpty(ConfigurationControls.MailSenderActivitySelector.selectedKey)
                                                                            ? "Mail sender is not selected"
                                                                            : string.Empty;
            return Task.FromResult(string.IsNullOrEmpty(ConfigurationControls.SalesforceObjectSelector.ErrorMessage)
                                && string.IsNullOrEmpty(ConfigurationControls.MailSenderActivitySelector.ErrorMessage));
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            //Lets first check if we just reconfigure initial solution UI or a user requested to prepare child activities
            var solutionBuildIsRequestedPreviously = !string.IsNullOrEmpty(this[nameof(ActivityUi.RunMailMergeButton)]);
            var solutionBuildIsRequested = ConfigurationControls.RunMailMergeButton.Clicked;
            //It means that user is still configuring UI OR he got back to design stage
            if (!solutionBuildIsRequested)
            {
                await ConfigureSolutionActivityUi();
            }
            //It means that this is reconfiguration caused by child activities
            else if (solutionBuildIsRequestedPreviously)
            {
                //TODO: what to do?
            }
            else
            //This means that user clicked on the button and we need to create child activities
            {
                await ConfigureChildActivities();
                this[nameof(ActivityUi.RunMailMergeButton)] = true.ToString();
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
            var loopActivity = await AddAndConfigureChildActivity(context.SolutionActivity, "Loop", "Loop", "Loop", 2);
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
            var sendEmailActivity = await AddAndConfigureChildActivity(loopActivity, solutionActivityUi.MailSenderActivitySelector.Value, order: 1);
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
                .Any(x => $"{x.ActivityTemplate.Name}_v{x.ActivityTemplate.Version}" == nameof(Get_Data_v1)
                     && x.Ordering == 1
                     && x.ActivityTemplate.TerminalId == terminalId);
            return Task.FromResult(result);
        }

        private async Task<ActivityDO> CreateSalesforceDataActivity(ReconfigurationContext context)
        {
            var getSalesforceDataActivityTemplate = (await HubCommunicator.GetActivityTemplates(null))
                                                     .Select(Mapper.Map<ActivityTemplateDO>)
                                                     .Where(x => $"{x.Name}_v{x.Version}" == nameof(Get_Data_v1)
                                                            && x.TerminalId == terminalId)
                                                     .First();
            var dataSourceActivity = await AddAndConfigureChildActivity(
                context.SolutionActivity,
                getSalesforceDataActivityTemplate.Id.ToString(),
                order: 1);
            return dataSourceActivity;
        }

        private async Task<ActivityDO> ConfigureSalesforceDataActivity(ReconfigurationContext context)
        {
            var activity = context.SolutionActivity
                                  .ChildNodes
                                  .OfType<ActivityDO>()
                                  .Single(x => x.Ordering == 1);
            using (var storage = CrateManager.GetUpdatableStorage(activity))
            {
                var controlsCrate = storage.FirstCrate<StandardConfigurationControlsCM>();
                var activityUi = new Get_Data_v1.ActivityUi().ClonePropertiesFrom(controlsCrate.Content) as Get_Data_v1.ActivityUi;
                var solutionActivityUi = new ActivityUi().ClonePropertiesFrom(CrateManager.GetStorage(context.SolutionActivity).FirstCrate<StandardConfigurationControlsCM>().Content) as ActivityUi;
                activityUi.SalesforceObjectSelector.selectedKey = solutionActivityUi.SalesforceObjectSelector.selectedKey;
                activityUi.SalesforceObjectSelector.Value = solutionActivityUi.SalesforceObjectSelector.Value;
                activityUi.SalesforceObjectFilter.Value = solutionActivityUi.SalesforceObjectFilter.Value;
                storage.ReplaceByLabel(Crate.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls), controlsCrate.Availability));
            }
            //Followup configuration
            activity = await HubCommunicator.ConfigureActivity(activity, CurrentFr8UserId);
            return activity;
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
            var selectedObjectProperties = await _salesforceManager.GetFields(selectedObject, AuthorizationToken);
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
            //No extra configuration required
            ConfigurationControls.SalesforceObjectSelector.ListItems = _salesforceManager.GetObjectDescriptions().Select(x => new ListItem { Key = x.Key, Value = x.Value }).ToList();
            ConfigurationControls.MailSenderActivitySelector.ListItems = (await HubCommunicator.GetActivityTemplates(ActivityTemplate.EmailDelivererTag, CurrentFr8UserId))
                                                                            .Select(x => new ListItem { Key = x.Label, Value = x.Name })
                                                                            .ToList();
        }

        protected override Task RunCurrentActivity()
        {
            throw new NotImplementedException();
        }
    }
}