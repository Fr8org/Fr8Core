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
using Hub.Enums;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class BuildQuery_v1 : BasePluginAction
    {
        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (Crate.IsEmptyStorage(curActionDTO.CrateStorage))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsCrate = Crate.GetStorage(curActionDTO).CratesOfType<StandardConfigurationControlsCM>().FirstOrDefault();

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

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                RemoveControl(updater.CrateStorage, "UpstreamError");

            var columnDefinitions = await ExtractColumnDefinitions(curActionDTO);
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
                return curActionDTO;
            }

                var controlsCrate = EnsureControlsCrate(updater.CrateStorage);

                AddSelectObjectDdl(updater.CrateStorage);
                AddLabelControl(updater.CrateStorage, "SelectObjectError", "No object selected", "Please select object from the list above.");

                updater.CrateStorage.RemoveByLabel("Available Tables");
                updater.CrateStorage.Add(Crate.CreateDesignTimeFieldsCrate("Available Tables", tablesList.ToArray()));
            }
            return curActionDTO;
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                RemoveControl(updater.CrateStorage, "SelectObjectError");

                var selectedObject = ExtractSelectedObject(updater.CrateStorage);
            if (string.IsNullOrEmpty(selectedObject))
            {
                    AddLabelControl(updater.CrateStorage, "SelectObjectError",
                    "No object selected", "Please select object from the list above.");

                return curActionDTO;
            }
            else
            {
                    var prevSelectedObject = ExtractPreviousSelectedObject(updater.CrateStorage);
                if (prevSelectedObject != selectedObject)
                {
                        RemoveControl(updater.CrateStorage, "SelectedQuery");
                        AddQueryBuilder(updater.CrateStorage);

                        UpdatePreviousSelectedObject(updater.CrateStorage, selectedObject);
                        await UpdateQueryableCriteria(updater.CrateStorage,  curActionDTO, selectedObject);
                }
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

            var tablesDefinitionCrate = upstreamCrates.FirstOrDefault(x => x.Label == "Sql Table Definitions");

            if (tablesDefinitionCrate == null) { return null; }

            var tablesDefinition = tablesDefinitionCrate.Get<StandardDesignTimeFieldsCM>();

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

            var columnTypes = columnTypesCrate.Get<StandardDesignTimeFieldsCM>();
                
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
        private void AddSelectObjectDdl(CrateStorage storage)
        {
            AddControl(
                storage,
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
        private string ExtractSelectedObject(CrateStorage storage)
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
        private string ExtractPreviousSelectedObject(CrateStorage storage)
            {
            var fields = storage.CratesOfType<StandardDesignTimeFieldsCM>().FirstOrDefault(x => x.Label == "Selected Object");

            if (fields == null || fields.Content.Fields.Count == 0)
            {
                return null;
            }

            return fields.Content.Fields[0].Key;
        }

        /// <summary>
        /// Update previously stored value of selected object type.
        /// </summary>
        private void UpdatePreviousSelectedObject(CrateStorage storage, string selectedObject)
        {
            UpdateDesignTimeCrateValue(
                storage,
                "Selected Object",
                new FieldDTO() { Key = selectedObject, Value = selectedObject }
            );
        }

        private async Task<List<FieldDTO>> MatchColumnsForSelectedObject(
            ActionDTO actionDTO, string selectedObject)
        {
            var findObjectHelper = new FindObjectHelper();

            var columnDefinitions = await ExtractColumnDefinitions(actionDTO);
            var columnTypeMap = await findObjectHelper.ExtractColumnTypes(this, actionDTO);

            if (columnDefinitions == null || columnTypeMap == null)
            {
                columnDefinitions = new List<FieldDTO>();
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
        private async Task UpdateQueryableCriteria(CrateStorage storage, ActionDTO actionDTO, string selectedObject)
        {
            var matchedColumns = await MatchColumnsForSelectedObject(actionDTO, selectedObject);
            UpdateDesignTimeCrateValue(storage, "Queryable Criteria", matchedColumns.ToArray());
        }

        /// <summary>
        /// Add query builder widget to action.
        /// </summary>
        private void AddQueryBuilder(CrateStorage storage)
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

            AddControl(storage, queryBuilder);
        }

        #endregion Configuration.

        #region Execution.

        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);
            var stroage = Crate.GetStorage(curActionDTO);
            var selectedObject = ExtractSelectedObject(stroage);
            if (string.IsNullOrEmpty(selectedObject))
            {
                throw new ApplicationException("No query object was selected.");
            }

            var queryBuilder = FindControl(stroage, "SelectedQuery");
            if (queryBuilder == null)
            {
                throw new ApplicationException("No QueryBuilder control found.");
            }

            var criteria = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(queryBuilder.Value);

            var sqlQueryCrate = CreateSqlQueryCrate(selectedObject, criteria);

            using (var updater = Crate.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(sqlQueryCrate);
            }

            return processPayload;
        }

        private Crate CreateSqlQueryCrate(
            string selectedObject,
            List<FilterConditionDTO> criteria)
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

            return Data.Crates.Crate.FromContent("Sql Query", standardQueryCM);
        }

        #endregion Execution.
    }
}