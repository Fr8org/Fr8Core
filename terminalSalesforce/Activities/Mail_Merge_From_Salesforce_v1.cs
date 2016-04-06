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
using terminalSalesforce.Actions;
using AutoMapper;
using Data.Interfaces;
using Utilities.Configuration.Azure;

namespace terminalSalesforce.Activities
{
    public class Mail_Merge_From_Salesforce_v1 : EnhancedTerminalActivity<Mail_Merge_From_Salesforce_v1.ActivityUi>
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
                    Required = true
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

        private const string QueryFilterCrateLabel = "Query Filter";

        private readonly ISalesforceManager _salesforceManager;

        private readonly int terminalId;

        public Mail_Merge_From_Salesforce_v1() : base(true)
        {
            ActivityName = "Mail Merge from Salesforce";
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalId = uow.TerminalRepository
                                .GetQuery()
                                .Where(x => x.Name == CloudConfigurationManager.GetSetting("TerminalName"))
                                .Select(x => x.Id)
                                .First();
            }
        }

        protected override Task<bool> Validate()
        {
            ConfigurationControls.SalesforceObjectSelector.ErrorMessage = string.IsNullOrEmpty(ConfigurationControls.SalesforceObjectSelector.selectedKey)
                                                                            ? "Object is not selected"
                                                                            : string.Empty;
            return Task.FromResult(string.IsNullOrEmpty(ConfigurationControls.SalesforceObjectSelector.ErrorMessage));
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

        private Task<bool> HasProcessingActivity(ReconfigurationContext arg)
        {
            throw new NotImplementedException();
        }

        private Task<ActivityDO> CreateProcessingActivity(ReconfigurationContext arg)
        {
            throw new NotImplementedException();
        }

        private Task<ActivityDO> ConfigureProcessingActivity(ReconfigurationContext arg)
        {
            throw new NotImplementedException();
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
            context.SolutionActivity.ChildNodes.Remove(dataSourceActivity);
            context.SolutionActivity.ChildNodes.Insert(0, dataSourceActivity);
            return dataSourceActivity;
        }

        private async Task<ActivityDO> ConfigureSalesforceDataActivity(ReconfigurationContext context)
        {
            var activity = context.SolutionActivity
                                  .ChildNodes
                                  .OfType<ActivityDO>()
                                  .Single(x => x.Ordering == 1);
            //Let activity configure its initial UI
            activity.CrateStorage = string.Empty;
            activity = await HubCommunicator.ConfigureActivity(activity, CurrentFr8UserId);
            //Now copy data from solution UI to this child activity UI and ask
            //TODO: this can be done properly after Get_Data activity is refactored into using ETA
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