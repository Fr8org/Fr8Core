using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;
using Data.Entities;
using Data.States;
using Data.Control;

namespace terminalFr8Core.Actions
{
    public class BuildQuery_v1 : BaseTerminalAction
    {
        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsCrate = Crate.GetStorage(curActionDO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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

        protected override async Task<ActionDO> InitialConfigurationResponse(
            ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                RemoveControl(updater.CrateStorage, "UpstreamError");

                var columnDefinitions = await ExtractColumnDefinitions(curActionDO);
                List<FieldDTO> tablesList = null;

                if (columnDefinitions != null)
                {
                    tablesList = ExtractTableNames(columnDefinitions);
                }

                if (tablesList == null || tablesList.Count == 0)
                {
                    AddLabelControl(
                            updater.CrateStorage,
                        "UpstreamError",
                        "Unexpected error",
                        "No upstream crates found to extract table definitions."
                    );
                    return curActionDO;
                }

                var controlsCrate = EnsureControlsCrate(updater.CrateStorage);

                AddSelectObjectDdl(updater.CrateStorage);
                AddLabelControl(updater.CrateStorage, "SelectObjectError", "No object selected", "Please select object from the list above.");

                updater.CrateStorage.RemoveByLabel("Available Tables");
                updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("Available Tables", tablesList.ToArray()));
            }
            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                RemoveControl(updater.CrateStorage, "SelectObjectError");

                var selectedObject = ExtractSelectedObjectFromControl(updater.CrateStorage);
                if (string.IsNullOrEmpty(selectedObject))
                {
                    AddLabelControl(updater.CrateStorage, "SelectObjectError",
                    "No object selected", "Please select object from the list above.");

                    return curActionDO;
                }
                else
                {
                    var prevSelectedObject = ExtractSelectedObjectFromCrate(updater.CrateStorage);
                    if (prevSelectedObject != selectedObject)
                    {
                        RemoveControl(updater.CrateStorage, "SelectedQuery");
                        AddQueryBuilder(updater.CrateStorage);

                        await UpdateQueryableCriteria(updater.CrateStorage,  curActionDO, selectedObject);
                    }
                }

                UpdateSelectedObjectCrate(updater.CrateStorage, selectedObject);
                UpdateSelectedQueryCrate(updater.CrateStorage);
            }

            return curActionDO;
        }

        /// <summary>
        /// Returns list of FieldDTO from upper crate from ConnectToSql action.
        /// </summary>
        private async Task<List<FieldDTO>> ExtractColumnDefinitions(ActionDO actionDO)
        {
            var upstreamCrates = await GetCratesByDirection<StandardDesignTimeFieldsCM>(
                actionDO,
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
        private void AddSelectObjectDdl(CrateStorage storage)
        {
            AddControl(
                storage,
                new DropDownList()
                {
                    Label = "Select Object",
                    Name = "SelectObjectDdl",
                    Required = true,
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onChange", "requestConfig")
                    },
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
        private string ExtractSelectedObjectFromControl(CrateStorage storage)
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
        private string ExtractSelectedObjectFromCrate(CrateStorage storage)
        {
            var fields = storage.CratesOfType<StandardDesignTimeFieldsCM>()
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
        private void UpdateSelectedObjectCrate(CrateStorage storage, string selectedObject)
        {
            UpdateDesignTimeCrateValue(
                storage,
                "Selected Object",
                new FieldDTO() { Key = selectedObject, Value = selectedObject }
            );
        }

        private StandardQueryCM ExtractSelectedQueryFromCrate(CrateStorage storage)
        {
            var queryCM = storage
                .CrateContentsOfType<StandardQueryCM>(x => x.Label == "Selected Query")
                .FirstOrDefault();

            return queryCM;
        }

        /// <summary>
        /// Update Selected Query crate.
        /// </summary>
        private void UpdateSelectedQueryCrate(CrateStorage storage)
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
            ActionDO actionDO, string selectedObject)
        {
            var findObjectHelper = new FindObjectHelper();

            var columnDefinitions = await ExtractColumnDefinitions(actionDO);
            var columnTypeMap = await findObjectHelper.ExtractColumnTypes(this, actionDO);

            var matchedColumns = findObjectHelper.MatchColumnsForSelectedObject(
                columnDefinitions, selectedObject, columnTypeMap);

            return matchedColumns;
        }

        /// <summary>
        /// Update queryable criteria list.
        /// </summary>
        private async Task UpdateQueryableCriteria(CrateStorage storage, ActionDO actionDO, string selectedObject)
        {
            var matchedColumns = await MatchColumnsForSelectedObject(actionDO, selectedObject);
            UpdateDesignTimeCrateValue(storage, "Queryable Criteria", matchedColumns.ToArray());
        }

        /// <summary>
        /// Add query builder widget to action.
        /// </summary>
        private void AddQueryBuilder(CrateStorage storage)
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

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActionDO, containerId);

            var actionCrateStorage = Crate.GetStorage(curActionDO);
            
            var sqlQueryCM = ExtractSelectedQueryFromCrate(actionCrateStorage);
            if (sqlQueryCM == null)
            {
                return Error(payloadCrates, "Selected Query crate was not found in Action's CrateStorage");
            }

            var sqlQueryCrate = Crate<StandardQueryCM>.FromContent("Sql Query", sqlQueryCM);

            using (var updater = Crate.UpdateStorage(payloadCrates))
            {
                updater.CrateStorage.Add(sqlQueryCrate);
            }

            return Success(payloadCrates);
        }

        #endregion Execution.
    }
}