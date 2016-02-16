﻿using System;
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
            if (Crate.IsStorageEmpty(curActivityDO))
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
            var availableSalesforceObjects = Crate.CreateDesignTimeFieldsCrate("AvailableSalesforceObjects",
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
                    new FieldDTO("Product") {Availability = AvailabilityType.Configuration},
                    new FieldDTO("Document") {Availability = AvailabilityType.Configuration}
                    //new FieldDTO("File") {Availability = AvailabilityType.Configuration}
                });

            //Note: This design time fields are used to populate the Fileter Pane controls. It has to be labelled as Queryable Criteria
            var emptyFieldsSource = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", new FieldDTO[] {});

            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                updater.CrateStorage = AssembleCrateStorage(availableSalesforceObjects, emptyFieldsSource, configurationControlsCrate);
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
            var queryableCriteriaFields = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", objectFieldsList.ToArray());
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.ReplaceByLabel(queryableCriteriaFields);
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
            var filterValue = ExtractControlFieldValue(curActivityDO, "SelectedFilter");
            var filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(filterValue);

            //if without filter, just get all selected objects
            //else prepare SOQL query to filter the objects based on the filter conditions
            StandardPayloadDataCM resultObjects;
            if (filterDataDTO.ExecutionType == FilterExecutionType.WithoutFilter)
            {
                resultObjects = await _salesforce.GetObjectByQuery(curSelectedSalesForceObject, string.Empty, _salesforce.CreateForceClient(authTokenDO));
            }
            else
            {
                EventManager.CriteriaEvaluationStarted(containerId);

                string parsedCondition = ParseFilters(filterDataDTO);

                resultObjects = await _salesforce.GetObjectByQuery(curSelectedSalesForceObject, parsedCondition, _salesforce.CreateForceClient(authTokenDO));
            }

            //update the payload with result objects
            using (var updater = Crate.UpdateStorage(payloadCrates))
            {
                updater.CrateStorage.ReplaceByLabel(Data.Crates.Crate.FromContent("Salesforce Objects", resultObjects));
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
                Label = "What kind of Salesforce object do you want to get data?",
                Source = new FieldSourceDTO
                {
                    Label = "AvailableSalesforceObjects",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
            };

            //Filter Pane control for the user to filter objects based on their fields values
            var fieldFilterPane = new FilterPane()
            {
                Label = "Get Salesforce Object Data by filtering Fields values",
                Name = "SelectedFilter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            return PackControlsCrate(whatKindOfData, fieldFilterPane);
        }

        private string ParseFilters(FilterDataDTO filterData)
        {
            var parsedConditions = new List<string>();

            filterData.Conditions.ForEach(condition =>
            {
                string parsedCondition = condition.Field;

                switch (condition.Operator)
                {
                    case "eq":
                        parsedCondition += " = ";
                        break;
                    case "neq":
                        parsedCondition += " != ";
                        break;
                    case "gt":
                        parsedCondition += " > ";
                        break;
                    case "gte":
                        parsedCondition += " >= ";
                        break;
                    case "lt":
                        parsedCondition += " < ";
                        break;
                    case "lte":
                        parsedCondition += " <= ";
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Not supported operator: {0}", condition.Operator));
                }

                parsedCondition += string.Format("'{0}'", condition.Value);
                parsedConditions.Add(parsedCondition);
            });

            var finalCondition = string.Join(" AND ", parsedConditions);

            return finalCondition;
        }
    }
}