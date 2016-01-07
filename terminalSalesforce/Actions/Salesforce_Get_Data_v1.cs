
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using Data.Interfaces.DataTransferObjects;

namespace terminalSalesforce.Actions
{
    public class Salesforce_Get_Data_v1 : BaseTerminalAction
    {
        ISalesforceIntegration _salesforce = new SalesforceIntegration();

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            //if empty crate storage, proceed with initial config
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            //if no salesforce object is selected, proceed with initial config
            string selectedSalesForceObject =
                ((DropDownList) GetControl(curActionDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;
            if (string.IsNullOrEmpty(selectedSalesForceObject))
            {
                return ConfigurationRequestType.Initial;
            }

            //proceed with follow up conifig if the above use cases are failed.
            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //create hard coded salesforce object names as design time fields.
            var availableSalesforceObjects = Crate.CreateDesignTimeFieldsCrate("AvailableSalesforceObjects",
                new FieldDTO[] { new FieldDTO("Account"), new FieldDTO("Contact"), new FieldDTO("Lead") });

            //Note: This design time fields are used to populate the Fileter Pane controls. It has to be labelled as Queryable Criteria
            var emptyFieldsSource = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", new FieldDTO[] {});

            //create required controls
            var configurationControlsCrate = CreateControlsCrate();

            //update the storage
            using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
            {
                updater.CrateStorage = AssembleCrateStorage(availableSalesforceObjects, emptyFieldsSource, configurationControlsCrate);
            }

            return await Task.FromResult(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO,
                                                                              AuthorizationTokenDO authTokenDO)
        {
            //get the current user selected salesforce object from the drop down list
            string curSelectedObject =
                ((DropDownList) GetControl(curActionDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;

            //if current selected object is empty , do not do anything
            if (string.IsNullOrEmpty(curSelectedObject))
            {
                return await Task.FromResult(curActionDO);
            }

            //get fields of selected salesforce object
            var objectFieldsList = await _salesforce.GetFields(curActionDO, authTokenDO, curSelectedObject);

            //update the crate storage
            var queryableCriteriaFields = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", objectFieldsList.ToArray());
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.ReplaceByLabel(queryableCriteriaFields);
            }

            return await Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            //get payload data
            var payloadCrates = await GetPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            //get currect selected Salesforce object
            string curSelectedSalesForceObject =
                ((DropDownList)GetControl(curActionDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;

            if (string.IsNullOrEmpty(curSelectedSalesForceObject))
            {
                return Error(payloadCrates, "No Salesforce object is selected by user");
            }

            //get filters
            var filterValue = GetControl(curActionDO, "SelectedFilter", ControlTypes.FilterPane).Value;
            var filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(filterValue);

            //if without filter, just get all selected objects
            //else prepare SOQL query to filter the objects based on the filter conditions
            StandardPayloadDataCM resultObjects;
            if (filterDataDTO.ExecutionType == FilterExecutionType.WithoutFilter)
            {
                resultObjects = await _salesforce.GetObject(curActionDO, authTokenDO, curSelectedSalesForceObject, string.Empty);
            }
            else
            {
                EventManager.CriteriaEvaluationStarted(containerId);

                string parsedCondition = ParseFilters(filterDataDTO);

                resultObjects = await _salesforce.GetObject(curActionDO, authTokenDO, curSelectedSalesForceObject, parsedCondition);
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