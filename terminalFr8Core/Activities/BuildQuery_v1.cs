using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;
using Data.Entities;
using Data.States;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace terminalFr8Core.Actions
{
    public class BuildQuery_v1 : BaseTerminalActivity
    {
        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsCrate = CrateManager.GetStorage(curActivityDO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsCrate == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var hasSelectObjectDdl = controlsCrate.Content.Controls
                .Any(x => x.Name == "SelectObjectDdl");

            if (!hasSelectObjectDdl)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(
            ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                RemoveControl(crateStorage, "UpstreamError");

                var columnDefinitions = await ExtractColumnDefinitions(curActivityDO);
                List<FieldDTO> tablesList = null;

                if (columnDefinitions != null)
                {
                    tablesList = ExtractTableNames(columnDefinitions);
                }

                if (tablesList == null || tablesList.Count == 0)
                {
                    AddLabelControl(
                            crateStorage,
                        "UpstreamError",
                        "Unexpected error",
                        "No upstream crates found to extract table definitions."
                    );
                    return curActivityDO;
                }

                var controlsCrate = EnsureControlsCrate(crateStorage);

                AddSelectObjectDdl(crateStorage);
                AddLabelControl(crateStorage, "SelectObjectError", "No object selected", "Please select object from the list above.");

                crateStorage.RemoveByLabel("Available Tables");
                crateStorage.Add(CrateManager.CreateDesignTimeFieldsCrate("Available Tables", tablesList.ToArray()));
            }
            return curActivityDO;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                RemoveControl(crateStorage, "SelectObjectError");

                var selectedObject = ExtractSelectedObjectFromControl(crateStorage);
                if (string.IsNullOrEmpty(selectedObject))
                {
                    AddLabelControl(crateStorage, "SelectObjectError",
                    "No object selected", "Please select object from the list above.");

                    return curActivityDO;
                }
                else
                {
                    var prevSelectedObject = ExtractSelectedObjectFromCrate(crateStorage);
                    if (prevSelectedObject != selectedObject)
                    {
                        RemoveControl(crateStorage, "SelectedQuery");
                        AddQueryBuilder(crateStorage);

                        await UpdateQueryableCriteria(crateStorage,  curActivityDO, selectedObject);
                    }
                }

                UpdateSelectedObjectCrate(crateStorage, selectedObject);
                UpdateSelectedQueryCrate(crateStorage);
            }

            return curActivityDO;
        }

        /// <summary>
        /// Returns list of FieldDTO from upper crate from ConnectToSql action.
        /// </summary>
        private async Task<List<FieldDTO>> ExtractColumnDefinitions(ActivityDO activityDO)
        {
            var upstreamCrates = await GetCratesByDirection<FieldDescriptionsCM>(
                activityDO,
                CrateDirection.Upstream
            );

            if (upstreamCrates == null) { return null; }

            var tablesDefinitionCrate = upstreamCrates.FirstOrDefault(x => x.Label == "Sql Table Definitions");

            if (tablesDefinitionCrate == null) { return null; }

            var tablesDefinition = tablesDefinitionCrate.Content;

            if (tablesDefinition == null) { return null; }

            return tablesDefinition.Fields;
        }

        /// <summary>
        /// Returns distinct list of table names from Table Definitions list.
        /// </summary>
        private List<FieldDTO> ExtractTableNames(List<FieldDTO> tableDefinitions)
        {
            var tables = new HashSet<string>();

            foreach (var column in tableDefinitions)
            {
                var columnTokens = column.Key
                    .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                string tableName;
                if (columnTokens.Length == 2)
                {
                    tableName = columnTokens[0];
                }
                else if (columnTokens.Length == 3)
                {
                    tableName = string.Format("{0}.{1}", columnTokens[0], columnTokens[1]);
                }
                else
                {
                    throw new NotSupportedException("Invalid column name.");
                }

                tables.Add(tableName);
            }

            var result = tables
                .Select(x => new FieldDTO() { Key = x, Value = x })
                .OrderBy(x => x.Key)
                .ToList();

            return result;
        }

        /// <summary>
        /// Add SelectObject drop-down-list to controls crate.
        /// </summary>
        private void AddSelectObjectDdl(ICrateStorage storage)
        {
            AddControl(
                storage,
                new DropDownList()
                {
                    Label = "Select Object",
                    Name = "SelectObjectDdl",
                    Required = true,
                    Events = new List<ControlEvent>(){ControlEvent.RequestConfig},
                    Source = new FieldSourceDTO
                    {
                        Label = "Available Tables",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }
            );
        }

        /// <summary>
        /// Extract SelectedObject from Action crates.
        /// </summary>
        private string ExtractSelectedObjectFromControl(ICrateStorage storage)
        {
            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controls == null) { return null; }

            var selectObjectDdl = controls.Controls.FirstOrDefault(x => x.Name == "SelectObjectDdl");
            if (selectObjectDdl == null) { return null; }

            return selectObjectDdl.Value;
        }

        /// <summary>
        /// Exract previously stored valued of selected object type.
        /// </summary>
        private string ExtractSelectedObjectFromCrate(ICrateStorage storage)
        {
            var fields = storage.CratesOfType<FieldDescriptionsCM>()
                .FirstOrDefault(x => x.Label == "Selected Object");

            if (fields == null || fields.Content.Fields.Count == 0)
            {
                return null;
            }

            return fields.Content.Fields[0].Key;
        }

        /// <summary>
        /// Update previously stored value of selected object type.
        /// </summary>
        private void UpdateSelectedObjectCrate(ICrateStorage storage, string selectedObject)
        {
            UpdateDesignTimeCrateValue(
                storage,
                "Selected Object",
                new FieldDTO() { Key = selectedObject, Value = selectedObject }
            );
        }

        private StandardQueryCM ExtractSelectedQueryFromCrate(ICrateStorage storage)
        {
            var queryCM = storage
                .CrateContentsOfType<StandardQueryCM>(x => x.Label == "Selected Query")
                .FirstOrDefault();

            return queryCM;
        }

        /// <summary>
        /// Update Selected Query crate.
        /// </summary>
        private void UpdateSelectedQueryCrate(ICrateStorage storage)
        {
            var selectedObject = ExtractSelectedObjectFromCrate(storage);

            var queryCrate = storage
                .CratesOfType<StandardQueryCM>(x => x.Label == "Selected Query")
                .FirstOrDefault();

            var queryBuilder = FindControl(storage, "SelectedQuery");
            if (queryBuilder == null)
            {
                if (queryCrate != null)
                {
                    storage.Remove(queryCrate);
                }
                return;
            }

            List<FilterConditionDTO> criteria;
            if (queryBuilder.Value != null)
            {
                criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(queryBuilder.Value);
            }
            else
            {
                criteria = new List<FilterConditionDTO>();
            }

            if (queryCrate == null)
            {
                queryCrate = Crate<StandardQueryCM>.FromContent(
                    "Selected Query",
                    new StandardQueryCM()
                    {
                        Queries = new List<QueryDTO>()
                        {
                            new QueryDTO()
                            {
                                Name = selectedObject,
                                Criteria = criteria
                            }
                        }
                    }
                );

                storage.Add(queryCrate);
            }

            queryCrate.Content.Queries = new List<QueryDTO>()
            {
                new QueryDTO()
                {
                    Name = selectedObject,
                    Criteria = criteria
                }
            };
        }

        private async Task<List<FieldDTO>> MatchColumnsForSelectedObject(
            ActivityDO activityDO, string selectedObject)
        {
            var findObjectHelper = new FindObjectHelper();

            var columnDefinitions = await ExtractColumnDefinitions(activityDO);
            var columnTypeMap = await findObjectHelper.ExtractColumnTypes(this, activityDO);

            var matchedColumns = findObjectHelper.MatchColumnsForSelectedObject(
                columnDefinitions, selectedObject, columnTypeMap);

            return matchedColumns;
        }

        /// <summary>
        /// Update queryable criteria list.
        /// </summary>
        private async Task UpdateQueryableCriteria(ICrateStorage storage, ActivityDO activityDO, string selectedObject)
        {
            var matchedColumns = await MatchColumnsForSelectedObject(activityDO, selectedObject);

            // TODO: FR-2347, fix here.
            UpdateDesignTimeCrateValue(storage, "Queryable Criteria", matchedColumns.ToArray());
        }

        /// <summary>
        /// Add query builder widget to action.
        /// </summary>
        private void AddQueryBuilder(ICrateStorage storage)
        {
            var queryBuilder = new QueryBuilder()
            {
                Label = "Please, specify query:",
                Name = "SelectedQuery",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            AddControl(storage, queryBuilder);
        }

        #endregion Configuration.

        #region Execution.

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            var actionCrateStorage = CrateManager.GetStorage(curActivityDO);
            
            var sqlQueryCM = ExtractSelectedQueryFromCrate(actionCrateStorage);
            if (sqlQueryCM == null)
            {
                return Error(payloadCrates, "Selected Query crate was not found in Action's CrateStorage");
            }

            var sqlQueryCrate = Crate<StandardQueryCM>.FromContent("Sql Query", sqlQueryCM);

            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(sqlQueryCrate);
            }

            return Success(payloadCrates);
        }

        #endregion Execution.

        public BuildQuery_v1(bool isAuthenticationRequired) : base(isAuthenticationRequired)
        {
        }

        protected override ActivityTemplateDTO MyTemplate { get; }
        public override Task Run()
        {
            throw new NotImplementedException();
        }

        public override Task Initialize()
        {
            throw new NotImplementedException();
        }

        public override Task FollowUp()
        {
            throw new NotImplementedException();
        }
    }
}