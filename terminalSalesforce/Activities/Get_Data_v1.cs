using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using Data.Interfaces.DataTransferObjects;
using Hub.Infrastructure;

namespace terminalSalesforce.Actions
{
    public class Get_Data_v1 : BaseTerminalActivity
    {
        private ISalesforceManager _salesforce;

        public Get_Data_v1()
        {
            _salesforce = ObjectFactory.GetInstance<ISalesforceManager>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            //if empty crate storage, proceed with initial config
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            //if no salesforce object is selected, proceed with initial config
            string selectedSalesForceObject =
                ((DropDownList) GetControl(curActivityDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;
            if (string.IsNullOrEmpty(selectedSalesForceObject))
            {
                return ConfigurationRequestType.Initial;
            }

            //proceed with follow up conifig if the above use cases are failed.
            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            //create hard coded salesforce object names as design time fields.
            var availableSalesforceObjects = CrateManager.CreateDesignTimeFieldsCrate("AvailableSalesforceObjects",
                new FieldDTO[]
                {
                    new FieldDTO("Account") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Contact") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Lead") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Opportunity") {Availability = AvailabilityType.Configuration},
                    //new FieldDTO("Forecast") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Contract") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Order") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Case") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Solution") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Product2") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Document") {Availability = AvailabilityType.Configuration}
                    //new FieldDTO("File") {Availability = AvailabilityType.Configuration}
                });

            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                crateStorage.Replace(AssembleCrateStorage(availableSalesforceObjects, configurationControlsCrate));
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO,
                                                                              AuthorizationTokenDO authTokenDO)
        {
            //get the current user selected salesforce object from the drop down list
            string curSelectedObject =
                ((DropDownList) GetControl(curActivityDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;

            //if current selected object is empty , do not do anything
            if (string.IsNullOrEmpty(curSelectedObject))
            {
                return await Task.FromResult(curActivityDO);
            }

            //get fields of selected salesforce object
            var objectFieldsList = await _salesforce.GetFields(curSelectedObject, _salesforce.CreateForceClient(authTokenDO));

            //replace the object fields for the newly selected object name
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                //Note: This design time fields are used to populate the Fileter Pane controls. It has to be labelled as Queryable Criteria
                crateStorage.RemoveByLabel("Queryable Criteria");
                crateStorage.Add(
                    Crate.FromContent("Queryable Criteria", new StandardQueryFieldsCM(
                        objectFieldsList.OrderBy(field => field.Key)
                                        .Select(field => new QueryFieldDTO(field.Key, field.Value, QueryFieldType.String, new TextBox { Name = field.Key })))));

                //FR-2459 - The activity should create another design time fields crate of type FieldDescriptionsCM for downstream activities.
                crateStorage.RemoveByLabel("Salesforce Object Fields");                                                                                     
                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Salesforce Object Fields", objectFieldsList.ToList(), AvailabilityType.RunTime));
            }

            return await Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            //get payload data
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            //get currect selected Salesforce object
            string curSelectedSalesForceObject =
                ((DropDownList)GetControl(curActivityDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;

            if (string.IsNullOrEmpty(curSelectedSalesForceObject))
            {
                return Error(payloadCrates, "No Salesforce object is selected by user");
            }

            //get filters
            var filterValue = ExtractControlFieldValue(curActivityDO, "SelectedQuery");
            var filterDataDTO = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(filterValue);

            //if without filter, just get all selected objects
            //else prepare SOQL query to filter the objects based on the filter conditions
            StandardPayloadDataCM resultObjects;
            if (!filterDataDTO.Any())
            {
                resultObjects = await _salesforce.GetObjectByQuery(curSelectedSalesForceObject, string.Empty, _salesforce.CreateForceClient(authTokenDO));
            }
            else
            {
                EventManager.CriteriaEvaluationStarted(containerId);


                string parsedCondition = ParseConditionToText(filterDataDTO);

                resultObjects = await _salesforce.GetObjectByQuery(curSelectedSalesForceObject, parsedCondition, _salesforce.CreateForceClient(authTokenDO));
            }

            //update the payload with result objects
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.ReplaceByLabel(Data.Crates.Crate.FromContent("Salesforce Objects", resultObjects));
            }

            return Success(payloadCrates);
        }

        private Crate CreateControlsCrate()
        {
            //DDLB for What Salesforce Object to be considered
            var whatKindOfData = new DropDownList
            {
                Name = "WhatKindOfData",
                Required = true,
                Label = "Get Which Object?",
                Source = new FieldSourceDTO
                {
                    Label = "AvailableSalesforceObjects",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
            };

            //Filter Pane control for the user to filter objects based on their fields values
            var queryBuilderPane = new QueryBuilder()
            {
                Label = "Meeting which conditions?",
                Name = "SelectedQuery",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifestTypes.StandardQueryFields
                }
            };

            var textArea = new TextArea()
            {
                //Setting the name as UT fails when checking the controls
                Name = string.Empty,
                IsReadOnly = true,
                Label = "",
                Value = "<p>Meeting which conditions?</p>"
            };

            //var textBlock = GenerateTextBlock("Meeting which conditions?","", "w");

            return PackControlsCrate(whatKindOfData, textArea, queryBuilderPane);
        }

        
    }
}