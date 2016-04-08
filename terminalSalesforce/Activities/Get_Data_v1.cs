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
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

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
                ((DropDownList)GetControl(curActivityDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;
            if (string.IsNullOrEmpty(selectedSalesForceObject))
            {
                return ConfigurationRequestType.Initial;
            }

            //proceed with follow up conifig if the above use cases are failed.
            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var configurationCrate = CreateControlsCrate();
            ActivitiesHelper.GetAvailableFields(configurationCrate, "WhatKindOfData");

            using (var crateStorage = CrateManager.UpdateStorage(() => curActivityDO.CrateStorage))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationCrate));
            }

            return await Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO,
                                                                              AuthorizationTokenDO authTokenDO)
        {
            //get the current user selected salesforce object from the drop down list
            string curSelectedObject =
                ((DropDownList)GetControl(curActivityDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;

            //if current selected object is empty , do not do anything
            if (string.IsNullOrEmpty(curSelectedObject))
            {
                return await Task.FromResult(curActivityDO);
            }

            if (CrateManager.GetStorage(curActivityDO).CratesOfType<FieldDescriptionsCM>().Any(x => x.Label.EndsWith(" - " + curSelectedObject)))
            {
                return await Task.FromResult(curActivityDO);
            }

            //get fields of selected salesforce object
            var objectFieldsList = await _salesforce.GetFields(curSelectedObject, authTokenDO);
            
            //replace the object fields for the newly selected object name
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                //Note: This design time fields are used to populate the Fileter Pane controls. It has to be labelled as Queryable Criteria
                crateStorage.RemoveByLabel("Queryable Criteria");
                crateStorage.Add(
                    Crate.FromContent("Queryable Criteria", new TypedFieldsCM(
                        objectFieldsList.OrderBy(field => field.Key)
                                        .Select(field => new TypedFieldDTO(field.Value, field.Key, FieldType.String, new TextBox { Name = field.Value })))));

                //FR-2459 - The activity should create another design time fields crate of type FieldDescriptionsCM for downstream activities.
                crateStorage.RemoveByLabelPrefix("Salesforce Object Fields");
                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Salesforce Object Fields - " + curSelectedObject, objectFieldsList.ToList(), AvailabilityType.RunTime));
            }

            return await Task.FromResult(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            //get payload data
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            //get currect selected Salesforce object
            string curSelectedSalesForceObject =
                ((DropDownList)GetControl(curActivityDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;

            var curSalesforceObjectFields = CrateManager.GetStorage( curActivityDO )
                                                         .CratesOfType<TypedFieldsCM>()
                                                         .Single(c => c.Label.Equals("Queryable Criteria"))
                                                         .Content.Fields.Select(f => f.Label);

            if (string.IsNullOrEmpty(curSelectedSalesForceObject))
            {
                return Error(payloadCrates, "No Salesforce object is selected by user");
            }

            //get filters
            var filterValue = ExtractControlFieldValue(curActivityDO, "SelectedQuery");
            var filterDataDTO = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(filterValue);

            //if without filter, just get all selected objects
            //else prepare SOQL query to filter the objects based on the filter conditions
            string parsedCondition = string.Empty;
            if (filterDataDTO.Any())
            {
                EventManager.CriteriaEvaluationStarted(containerId);
                parsedCondition = ParseConditionToText(filterDataDTO);
            }

            var resultObjects = await _salesforce.GetObjectByQuery(curSelectedSalesForceObject, curSalesforceObjectFields, parsedCondition, authTokenDO);

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
                Source = null,
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