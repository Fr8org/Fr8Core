using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;

namespace terminalFr8Core.Actions
{
    public class BuildQuery_v1 : BasePluginAction
    {
        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActionDO curActionDO)
        {
            if (curActionDO.CrateStorageDTO() == null
                || curActionDO.CrateStorageDTO().CrateDTO == null
                || curActionDO.CrateStorageDTO().CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsCrate = curActionDO.CrateStorageDTO().CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);

            if (controlsCrate == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var controls = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                controlsCrate.Contents);

            var hasSelectObjectDdl = controls.Controls
                .Any(x => x.Name == "SelectObjectDdl");

            if (!hasSelectObjectDdl)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            RemoveControl(curActionDO, "UpstreamError");

            var columnDefinitions = await ExtractColumnDefinitions(curActionDO);
            List<FieldDTO> tablesList = null;

            if (columnDefinitions != null)
            {
                tablesList = ExtractTableNames(columnDefinitions);
            }

            if (tablesList == null || tablesList.Count == 0)
            {
                AddLabelControl(
                    curActionDO,
                    "UpstreamError",
                    "Unexpected error",
                    "No upstream crates found to extract table definitions."
                );
                return curActionDO;
            }

            var controlsCrate = EnsureControlsCrate(curActionDO);
            AddSelectObjectDdl(curActionDO);
            AddLabelControl(curActionDO, "SelectObjectError",
                "No object selected", "Please select object from the list above.");

            curActionDO.CrateStorageDTO().CrateDTO.Add(
                Crate.CreateDesignTimeFieldsCrate("Available Tables", tablesList.ToArray())
            );

            return curActionDO;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO = null)
        {
            RemoveControl(curActionDO, "SelectObjectError");

            var selectedObject = ExtractSelectedObject(curActionDO);
            if (string.IsNullOrEmpty(selectedObject))
            {
                AddLabelControl(curActionDO, "SelectObjectError",
                    "No object selected", "Please select object from the list above.");

                return curActionDO;
            }
            else
            {
                var prevSelectedObject = ExtractPreviousSelectedObject(curActionDO);
                if (prevSelectedObject != selectedObject)
                {
                    RemoveControl(curActionDO, "SelectedQuery");
                    AddQueryBuilder(curActionDO);

                    UpdatePreviousSelectedObject(curActionDO, selectedObject);
                    await UpdateQueryableCriteria(curActionDO, selectedObject);
                }
            }

            return curActionDO;
        }

        /// <summary>
        /// Returns list of FieldDTO from upper crate from ConnectToSql action.
        /// </summary>
        private async Task<List<FieldDTO>> ExtractColumnDefinitions(ActionDO actionDO)
        {
            var upstreamCrates = await GetCratesByDirection(
                actionDO.Id,
                CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                GetCrateDirection.Upstream
            );

            if (upstreamCrates == null) { return null; }

            var tablesDefinitionCrate = upstreamCrates
                .FirstOrDefault(x => x.Label == "Sql Table Definitions");

            if (tablesDefinitionCrate == null) { return null; }

            var tablesDefinition = JsonConvert
                .DeserializeObject<StandardDesignTimeFieldsCM>(tablesDefinitionCrate.Contents);

            if (tablesDefinition == null) { return null; }

            return tablesDefinition.Fields;
        }

        private async Task<List<FieldDTO>> ExtractColumnTypes(ActionDO actionDO)
        {
            var upstreamCrates = await GetCratesByDirection(
                actionDO.Id,
                CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                GetCrateDirection.Upstream
            );

            if (upstreamCrates == null) { return null; }

            var columnTypesCrate = upstreamCrates
                .FirstOrDefault(x => x.Label == "Sql Column Types");

            if (columnTypesCrate == null) { return null; }

            var columnTypes = JsonConvert
                .DeserializeObject<StandardDesignTimeFieldsCM>(columnTypesCrate.Contents);

            if (columnTypes == null) { return null; }

            return columnTypes.Fields;
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
        private void AddSelectObjectDdl(ActionDO actionDO)
        {
            AddControl(
                actionDO,
                new DropDownListControlDefinitionDTO()
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
                        ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                    }
                }
            );
        }

        /// <summary>
        /// Extract SelectedObject from Action crates.
        /// </summary>
        private string ExtractSelectedObject(ActionDO actionDO)
        {
            var controlsCrate = actionDO.CrateStorageDTO().CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);

            if (controlsCrate == null) { return null; }

            var controls = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                controlsCrate.Contents);

            var selectObjectDdl = controls.Controls.FirstOrDefault(x => x.Name == "SelectObjectDdl");
            if (selectObjectDdl == null) { return null; }

            return selectObjectDdl.Value;
        }

        /// <summary>
        /// Exract previously stored valued of selected object type.
        /// </summary>
        private string ExtractPreviousSelectedObject(ActionDO actionDO)
        {
            var crate = actionDO.CrateStorageDTO().CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                    && x.Label == "Selected Object");

            if (crate == null)
            {
                return null;
            }

            var fields = JsonConvert.DeserializeObject<StandardDesignTimeFieldsCM>(crate.Contents);
            if (fields == null || fields.Fields.Count == 0)
            {
                return null;
            }

            return fields.Fields[0].Key;
        }

        /// <summary>
        /// Update previously stored value of selected object type.
        /// </summary>
        private void UpdatePreviousSelectedObject(ActionDO actionDO, string selectedObject)
        {
            UpdateDesignTimeCrateValue(
                actionDO,
                "Selected Object",
                new FieldDTO() { Key = selectedObject, Value = selectedObject }
            );
        }

        private async Task<List<FieldDTO>> MatchColumnsForSelectedObject(
            ActionDO actionDO, string selectedObject)
        {
            var columnDefinitions = await ExtractColumnDefinitions(actionDO);
            var columnTypes = await ExtractColumnTypes(actionDO);

            if (columnDefinitions == null || columnTypes == null)
            {
                columnDefinitions = new List<FieldDTO>();
            }

            // Create columnTypeMap dictionary.
            var columnTypeMap = new Dictionary<string, DbType>();
            foreach (var columnType in columnTypes)
            {
                columnTypeMap.Add(columnType.Key, (DbType)Enum.Parse(typeof(DbType), columnType.Value));
            }

            var supportedColumnTypes = new HashSet<DbType>() { DbType.String, DbType.Int32, DbType.Boolean };

            // Match columns and filter by supported column type.
            List<FieldDTO> matchedColumns;
            if (string.IsNullOrEmpty(selectedObject))
            {
                matchedColumns = new List<FieldDTO>();
            }
            else
            {
                matchedColumns = columnDefinitions
                    .Where(x => x.Key.StartsWith(selectedObject))
                    .Where(x => supportedColumnTypes.Contains(columnTypeMap[x.Key]))
                    .Select(x =>
                    {
                        var tokens = x.Key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        var columnName = tokens[tokens.Length - 1];

                        return new FieldDTO() { Key = columnName, Value = columnName };
                    })
                    .ToList();
            }

            return matchedColumns;
        }

        /// <summary>
        /// Update queryable criteria list.
        /// </summary>
        private async Task UpdateQueryableCriteria(ActionDO actionDO, string selectedObject)
        {
            var matchedColumns = await MatchColumnsForSelectedObject(actionDO, selectedObject);
            UpdateDesignTimeCrateValue(actionDO, "Queryable Criteria", matchedColumns.ToArray());
        }

        /// <summary>
        /// Add query builder widget to action.
        /// </summary>
        private void AddQueryBuilder(ActionDO actionDO)
        {
            var queryBuilder = new QueryBuilderControlDefinitionDTO()
            {
                Label = "Please, specify query:",
                Name = "SelectedQuery",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                }
            };

            AddControl(actionDO, queryBuilder);
        }

        #endregion Configuration.

        #region Execution.

        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            var processPayload = await GetProcessPayload(containerId);

            var selectedObject = ExtractSelectedObject(curActionDO);
            if (string.IsNullOrEmpty(selectedObject))
            {
                throw new ApplicationException("No query object was selected.");
            }

            var queryBuilder = FindControl(curActionDO, "SelectedQuery");
            if (queryBuilder == null)
            {
                throw new ApplicationException("No QueryBuilder control found.");
            }

            var criteria = JsonConvert.DeserializeObject<List<CriteriaDTO>>(queryBuilder.Value);

            var sqlQueryCrate = CreateSqlQueryCrate(selectedObject, criteria);

            processPayload.UpdateCrateStorageDTO(new List<CrateDTO>() { sqlQueryCrate });

            return processPayload;
        }

        private CrateDTO CreateSqlQueryCrate(string selectedObject, List<CriteriaDTO> criteria)
        {
            var query = new QueryDTO()
            {
                Name = selectedObject,
                Criteria = criteria
            };

            var standardQueryCM = new StandardQueryCM()
            {
                Queries = new List<QueryDTO>() { query }
            };

            var sqlQueryCrate = Crate.Create(
                "Sql Query",
                JsonConvert.SerializeObject(standardQueryCM),
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME,
                CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID
            );

            return sqlQueryCrate;
        }

        #endregion Execution.
    }
}