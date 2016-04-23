
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using System.Collections.Generic;
using TerminalBase.BaseClasses;
using System;
using System.Threading.Tasks;
using terminalSalesforce.Infrastructure;
using StructureMap;
using System.Linq;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Constants;
using System.Globalization;

namespace terminalSalesforce.Actions
{
    public class Monitor_Salesforce_Event_v1 : EnhancedTerminalActivity<Monitor_Salesforce_Event_v1.ActivityUi>
    {
        private const string CreatedEventname = "Created";
        private const string UpdatedEventname = "Updated";
        private const string SalesforceObjectFieldsCrateLabel = "Salesforce Object Fields";
        private const string RuntimeDataCrateLabel = "Table from Salesforce Get Data";

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

        ISalesforceManager _salesforceManager;

        public Monitor_Salesforce_Event_v1() : base(true)
        {
            _salesforceManager = ObjectFactory.GetInstance<ISalesforceManager>();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ActivitiesHelper.GetAvailableFields(ConfigurationControls.SalesforceObjectList);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            string curSfChosenObject = ConfigurationControls.SalesforceObjectList.selectedKey;

            if(string.IsNullOrEmpty(curSfChosenObject))
            {
                return;
            }

            var eventSubscriptionCrate = PackEventSubscriptionsCrate(ConfigurationControls);
            CurrentActivityStorage.ReplaceByLabel(eventSubscriptionCrate);

            CurrentActivityStorage.ReplaceByLabel(
                CrateManager.CreateDesignTimeFieldsCrate("Monitor Salesforce Fields", CreateSfMonitorDesignTimeFields().ToList(), AvailabilityType.RunTime));

            var eventCrate = CrateManager.CreateManifestDescriptionCrate(
                                            "Available Run-Time Objects",
                                            MT.SalesforceEvent.ToString(),
                                            ((int)MT.SalesforceEvent).ToString(CultureInfo.InvariantCulture),
                                            AvailabilityType.RunTime);

            var tableDataCrate = CrateManager.CreateManifestDescriptionCrate(
                                            "Available Run-Time Objects",
                                            MT.StandardTableData.ToString(),
                                            ((int)MT.StandardTableData).ToString(CultureInfo.InvariantCulture),
                                            AvailabilityType.RunTime);

            CurrentActivityStorage.RemoveByLabel("Available Run-Time Objects");
            CurrentActivityStorage.AddRange(new List<Crate> { eventCrate, tableDataCrate });

            var selectedObjectProperties = await _salesforceManager.GetFields(curSfChosenObject, AuthorizationToken);
            var objectPropertiesCrate = Crate<FieldDescriptionsCM>.FromContent(
                SalesforceObjectFieldsCrateLabel,
                new FieldDescriptionsCM(selectedObjectProperties),
                AvailabilityType.RunTime);
            CurrentActivityStorage.ReplaceByLabel(objectPropertiesCrate);
        }

        protected override async Task RunCurrentActivity()
        {
            //get the event payload from the Salesforce notification event
            var sfEventPayloads = CurrentPayloadStorage.CratesOfType<EventReportCM>().ToList().SelectMany(er => er.Content.EventPayload).ToList();

            //if the payload does not contain Salesforce Event Notificaiton Payload, then it means,
            //user initially runs this plan. Just acknowledge that the plan is activated successfully and it monitors the Salesforce events
            if (sfEventPayloads == null || 
                sfEventPayloads.Count == 0 || 
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
                CurrentPayloadStorage.ReplaceByLabel(
                    Crate.FromContent("Salesforce Event", sfEvent, AvailabilityType.RunTime));
            });

            //get the currently selected object fields
            var salesforceObjectFields = CurrentActivityStorage
                                            .FirstCrate<FieldDescriptionsCM>(x => x.Label == SalesforceObjectFieldsCrateLabel)
                                            .Content
                                            .Fields
                                            .Select(x => x.Key);

            //for each Salesforce event notification
            var sfEventsList = CurrentPayloadStorage.CrateContentsOfType<SalesforceEventCM>().ToList();
            foreach (var sfEvent in sfEventsList)
            {
                //get the object fields as Standard Table Data
                var resultObjects = await _salesforceManager.QueryObjects(
                                                sfEvent.ObjectType,
                                                salesforceObjectFields, 
                                                string.Format("ID = '{0}'", sfEvent.ObjectId), 
                                                AuthorizationToken);
                CurrentPayloadStorage.Add(Crate<StandardTableDataCM>.FromContent(RuntimeDataCrateLabel, resultObjects, AvailabilityType.RunTime));
            }
        }

        private Crate PackEventSubscriptionsCrate(ActivityUi curSfActivityUi)
        {
            var curSfChosenObject = curSfActivityUi.SalesforceObjectList.selectedKey;

            var eventSubscriptions = new List<string>();
            
            if (curSfActivityUi.Created.Selected)
            {
                eventSubscriptions.Add(string.Format("{0}{1}", curSfChosenObject, CreatedEventname));
            }
            if (curSfActivityUi.Updated.Selected)
            {
                eventSubscriptions.Add(string.Format("{0}{1}", curSfChosenObject, UpdatedEventname));
            }

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Salesforce",
                eventSubscriptions.ToArray());
        }

        private List<FieldDTO> CreateSfMonitorDesignTimeFields()
        {
            var monitorEventFields = new List<FieldDTO> {

                new FieldDTO("ObjectType", "ObjectType", AvailabilityType.RunTime),
                new FieldDTO("ObjectId", "ObjectId", AvailabilityType.RunTime),
                new FieldDTO("CreatedDate", "CreatedDate", AvailabilityType.RunTime),
                new FieldDTO("LastModifiedDate", "LastModifiedDate", AvailabilityType.RunTime),
                new FieldDTO("OccuredEvent", "OccuredEvent", AvailabilityType.RunTime)
            };

            return monitorEventFields;
        }
    }
}