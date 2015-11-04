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

namespace terminalFr8Core.Actions
{
    public class BuildQuery_v1 : BasePluginAction
    {
        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null
                || curActionDTO.CrateStorage.CrateDTO == null
                || curActionDTO.CrateStorage.CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsCrate = curActionDTO.CrateStorage.CrateDTO
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

        protected override async Task<ActionDTO> InitialConfigurationResponse(
            ActionDTO curActionDTO)
        {
            RemoveControl(curActionDTO, "UpstreamError");

            var columnDefinitions = await ExtractColumnDefinitions(curActionDTO);
            List<FieldDTO> tablesList = null;

            if (columnDefinitions != null)
            {
                tablesList = ExtractTableNames(columnDefinitions);
            }

            if (tablesList == null || tablesList.Count == 0)
            {
                AddLabelControl(
                    curActionDTO,
                    "UpstreamError",
                    "Unexpected error",
                    "No upstream crates found to extract table definitions."
                );
                return curActionDTO;
            }

            var controlsCrate = EnsureControlsCrate(curActionDTO);
            AddSelectObjectDdl(curActionDTO);
            AddLabelControl(curActionDTO, "SelectObjectError",
                "No object selected", "Please select object from the list above.");

            curActionDTO.CrateStorage.CrateDTO.Add(
                Crate.CreateDesignTimeFieldsCrate("Available Tables", tablesList.ToArray())
            );

            return curActionDTO;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(
            ActionDTO curActionDTO)
        {
            RemoveControl(curActionDTO, "SelectObjectError");

            var selectedObject = ExtractSelectedObject(curActionDTO);
            if (string.IsNullOrEmpty(selectedObject))
            {
                AddLabelControl(curActionDTO, "SelectObjectError",
                    "No object selected", "Please select object from the list above.");

                return curActionDTO;
            }
            else
            {
                var prevSelectedObject = ExtractPreviousSelectedObject(curActionDTO);
                if (prevSelectedObject != selectedObject)
                {
                    RemoveControl(curActionDTO, "SelectedQuery");
                    AddQueryBuilder(curActionDTO);

                    UpdatePreviousSelectedObject(curActionDTO, selectedObject);
                    await UpdateQueryableCriteria(curActionDTO, selectedObject);
                }
            }

            return curActionDTO;
        }

        /// <summary>
        /// Returns list of FieldDTO from upper crate from ConnectToSql action.
        /// </summary>
        private async Task<List<FieldDTO>> ExtractColumnDefinitions(ActionDTO actionDTO)
        {
            var upstreamCrates = await GetCratesByDirection(
                actionDTO.Id,
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

        private async Task<List<FieldDTO>> ExtractColumnTypes(ActionDTO actionDTO)
        {
            var upstreamCrates = await GetCratesByDirection(
                actionDTO.Id,
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
        private void AddSelectObjectDdl(ActionDTO actionDTO)
        {
            AddControl(
                actionDTO,
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
        private string ExtractSelectedObject(ActionDTO actionDTO)
        {
            var controlsCrate = actionDTO.CrateStorage.CrateDTO
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
        private string ExtractPreviousSelectedObject(ActionDTO actionDTO)
        {
            var crate = actionDTO.CrateStorage.CrateDTO
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
        private void UpdatePreviousSelectedObject(ActionDTO actionDTO, string selectedObject)
        {
            UpdateDesignTimeCrateValue(
                actionDTO,
                "Selected Object",
                new FieldDTO() { Key = selectedObject, Value = selectedObject }
            );
        }

        private async Task<List<FieldDTO>> MatchColumnsForSelectedObject(
            ActionDTO actionDTO, string selectedObject)
        {
            var columnDefinitions = await ExtractColumnDefinitions(actionDTO);
            var columnTypes = await ExtractColumnTypes(actionDTO);

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
        private async Task UpdateQueryableCriteria(ActionDTO actionDTO, string selectedObject)
        {
            var matchedColumns = await MatchColumnsForSelectedObject(actionDTO, selectedObject);
            UpdateDesignTimeCrateValue(actionDTO, "Queryable Criteria", matchedColumns.ToArray());
        }

        /// <summary>
        /// Add query builder widget to action.
        /// </summary>
        private void AddQueryBuilder(ActionDTO actionDTO)
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

            AddControl(actionDTO, queryBuilder);
        }

        #endregion Configuration.

        #region Execution.

        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);

            var selectedObject = ExtractSelectedObject(curActionDTO);
            if (string.IsNullOrEmpty(selectedObject))
            {
                throw new ApplicationException("No query object was selected.");
            }

            var queryBuilder = FindControl(curActionDTO, "SelectedQuery");
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