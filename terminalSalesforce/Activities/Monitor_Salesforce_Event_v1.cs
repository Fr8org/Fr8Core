
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using System.Collections.Generic;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using terminalSalesforce.Infrastructure;
using StructureMap;
using System.Linq;

namespace terminalSalesforce.Actions
{
    public class Monitor_Salesforce_Event_v1 : EnhancedTerminalActivity<Monitor_Salesforce_Event_v1.ActivityUi>
    {
        private const string CreatedEventname = "Created";
        private const string UpdatedEventname = "Updated";

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SalesforceObjectList { get; set; }

            public TextBlock EventDescription { get; set; }

            public CheckBox Created { get; set; }

            public CheckBox Updated { get; set; }

            public ActivityUi()
            {
                SalesforceObjectList = new DropDownList
                {
                    Label = "Which object do you want to monitor?",
                    Name = nameof(SalesforceObjectList),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Add(SalesforceObjectList);

                EventDescription = new TextBlock()
                {
                    Label = "Detect objects that have been: ",
                    Name = nameof(EventDescription)
                };
                Controls.Add(EventDescription);

                Created = new CheckBox
                {
                    Label = "Created",
                    Name = nameof(Created),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Selected = false
                };
                Controls.Add(Created);

                Updated = new CheckBox
                {
                    Label = "Updated",
                    Name = nameof(Updated),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Selected = false
                };
                Controls.Add(Updated);
            }
        }

        readonly ISalesforceManager _salesforceManager;

        public Monitor_Salesforce_Event_v1() : base(true)
        {
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
        }

        protected override Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ActivitiesHelper.GetAvailableFields(ConfigurationControls.SalesforceObjectList);

            return Task.FromResult(0);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            string curSfChosenObject = ConfigurationControls.SalesforceObjectList.selectedKey;

            if(string.IsNullOrEmpty(curSfChosenObject))
            {
                return;
            }

            var eventSubscriptionCrate = PackEventSubscriptionsCrate();

            CurrentActivityStorage.ReplaceByLabel(eventSubscriptionCrate);
            
            runtimeCrateManager.ClearAvailableCrates();
            runtimeCrateManager.MarkAvailableAtRuntime<SalesforceEventCM>("Salesforce Event");

            var selectedObjectProperties = await _salesforceManager.GetFields(curSfChosenObject, AuthorizationToken);

            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(GenerateRuntimeDataLabel()).AddFields(selectedObjectProperties);
        }

        protected override async Task RunCurrentActivity()
        {
            //get the event payload from the Salesforce notification event
            var sfEventPayloads = CurrentPayloadStorage.CratesOfType<EventReportCM>().ToList().SelectMany(er => er.Content.EventPayload).ToList();

            //if the payload does not contain Salesforce Event Notificaiton Payload, then it means,
            //user initially runs this plan. Just acknowledge that the plan is activated successfully and it monitors the Salesforce events
            if (sfEventPayloads.Count == 0 || 
                !sfEventPayloads.Any(payload => payload.Label.Equals("Salesforce Event Notification Payload")))
            {
                await Activate(CurrentActivity, AuthorizationToken);
                RequestHubExecutionTermination("Plan successfully activated. It will wait and respond to specified Salesforce Event messages.");
                return;
            }

            //if payload contains Salesforce Notification, get it from the payload storage
            var curEventReport = CurrentPayloadStorage.CratesOfType<EventReportCM>().Single(er => er.Content.Manufacturer.Equals("Salesforce")).Content;
            var curEventPayloads = curEventReport.EventPayload.CrateContentsOfType<StandardPayloadDataCM>().Single().PayloadObjects;

            //for each payload,
            curEventPayloads.ForEach(p =>
            {
                //create SalesforceEventCM with the values. The values are:
                //Object ID          -> Id of the newly created or updated Salesforce Object
                //Object Type        -> Type of the newly created or updated Salesforce Object (ex., Lead, Account or Contact
                //Occured Event      -> Cause of this notification (ex., Create or Updated)
                //Created Date       -> Date at which the object is created
                //LastModified Date  -> Date at which the object is last modified
                var sfEvent = new SalesforceEventCM
                {
                    ObjectId = p.PayloadObject.Single(requiredProperty => requiredProperty.Key.Equals("ObjectId")).Value,
                    ObjectType = p.PayloadObject.Single(requiredProperty => requiredProperty.Key.Equals("ObjectType")).Value,
                    OccuredEvent = p.PayloadObject.Single(requiredProperty => requiredProperty.Key.Equals("OccuredEvent")).Value,
                    CreatedDate = p.PayloadObject.Single(requiredProperty => requiredProperty.Key.Equals("CreatedDate")).Value,
                    LastModifiedDate = p.PayloadObject.Single(requiredProperty => requiredProperty.Key.Equals("LastModifiedDate")).Value                    
                };

                //store the SalesforceEventCM into the current payload
                CurrentPayloadStorage.ReplaceByLabel(Crate.FromContent("Salesforce Event", sfEvent));
            });

            var runtimeCrateLabel = GenerateRuntimeDataLabel();

            var salesforceObjectFields = CurrentActivityStorage.FirstCrate<CrateDescriptionCM>().Content.CrateDescriptions.First(x => x.Label == runtimeCrateLabel).Fields.Select(x => x.Key).ToArray();

            //for each Salesforce event notification
            var sfEventsList = CurrentPayloadStorage.CrateContentsOfType<SalesforceEventCM>().ToList();
            foreach (var sfEvent in sfEventsList)
            {
                //get the object fields as Standard Table Data
                var resultObjects = await _salesforceManager.QueryObjects(
                                                sfEvent.ObjectType,
                                                salesforceObjectFields,
                                                $"ID = '{sfEvent.ObjectId}'", 
                                                AuthorizationToken);

                CurrentPayloadStorage.Add(Crate<StandardTableDataCM>.FromContent(runtimeCrateLabel, resultObjects));
            }
        }

        private string GenerateRuntimeDataLabel()
        {
            var curSfChosenObject = ConfigurationControls.SalesforceObjectList.selectedKey;

            var eventSubscriptions = new List<string>();

            if (ConfigurationControls.Created.Selected)
            {
                eventSubscriptions.Add(CreatedEventname);
            }
            if (ConfigurationControls.Updated.Selected)
            {
                eventSubscriptions.Add(UpdatedEventname);
            }

            var modifiers = string.Join("/", eventSubscriptions);

            return $"{curSfChosenObject} {modifiers} on Salesforce.com";
        }

        private Crate PackEventSubscriptionsCrate()
        {
            var curSfChosenObject = ConfigurationControls.SalesforceObjectList.selectedKey;

            var eventSubscriptions = new List<string>();
            
            if (ConfigurationControls.Created.Selected)
            {
                eventSubscriptions.Add($"{curSfChosenObject}{CreatedEventname}");
            }
            if (ConfigurationControls.Updated.Selected)
            {
                eventSubscriptions.Add($"{curSfChosenObject}{UpdatedEventname}");
            }

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Salesforce",
                eventSubscriptions.ToArray());
        }
    }
}