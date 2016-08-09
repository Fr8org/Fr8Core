using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Data.Crates;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Activities
{
   public class Build_Query_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("00dc3a6e-3c08-4918-824f-d966d5ebfa91"),
            Name = "Build_Query",
            Label = "Build Query",
            Version = "1",
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        #region Configuration.

        /// <summary>
        /// Returns list of FieldDTO from upper crate from ConnectToSql action.
        /// </summary>
        private async Task<List<KeyValueDTO>> ExtractColumnDefinitions()
        {
            var upstreamCrates = await  HubCommunicator.GetCratesByDirection<KeyValueListCM>(ActivityId, CrateDirection.Upstream);
            var tablesDefinitionCrate = upstreamCrates?.FirstOrDefault(x => x.Label == "Sql Table Definitions");
            var tablesDefinition = tablesDefinitionCrate?.Content;
            return tablesDefinition?.Values;
        }

        /// <summary>
        /// Returns distinct list of table names from Table Definitions list.
        /// </summary>
        private List<KeyValueDTO> ExtractTableNames(List<KeyValueDTO> tableDefinitions)
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
                .Select(x => new KeyValueDTO() { Key = x, Value = x })
                .OrderBy(x => x.Key)
                .ToList();

            return result;
        }

        /// <summary>
        /// Add SelectObject drop-down-list to controls crate.
        /// </summary>
        private void AddSelectObjectDdl()
        {
            AddControl(
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
        private string ExtractSelectedObjectFromControl()
        {
            var selectObjectDdl = ConfigurationControls.Controls.FirstOrDefault(x => x.Name == "SelectObjectDdl");
            return selectObjectDdl?.Value;
        }
        
        private StandardQueryCM ExtractSelectedQueryFromCrate()
        {
            var queryCM = Storage.CrateContentsOfType<StandardQueryCM>(x => x.Label == "Selected Query")
                .FirstOrDefault();

            return queryCM;
        }

        /// <summary>
        /// Update Selected Query crate.
        /// </summary>
        private void UpdateSelectedQueryCrate()
        {
            var selectedObject = this["Selected Object"];

            var queryCrate = Storage
                .CratesOfType<StandardQueryCM>(x => x.Label == "Selected Query")
                .FirstOrDefault();

            var queryBuilder = GetControl<QueryBuilder>("SelectedQuery");
            if (queryBuilder == null)
            {
                if (queryCrate != null)
                {
                    Storage.Remove(queryCrate);
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
                queryCrate = Fr8.Infrastructure.Data.Crates.Crate<StandardQueryCM>.FromContent(
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

                Storage.Add(queryCrate);
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

        private async Task<List<KeyValueDTO>> MatchColumnsForSelectedObject(string selectedObject)
        {
            var findObjectHelper = new FindObjectHelper();

            var columnDefinitions = await ExtractColumnDefinitions();
            var columnTypeMap = await findObjectHelper.ExtractColumnTypes(HubCommunicator, ActivityContext);

            var matchedColumns = findObjectHelper.MatchColumnsForSelectedObject(
                columnDefinitions, selectedObject, columnTypeMap);

            return matchedColumns;
        }

        /// <summary>
        /// Update queryable criteria list.
        /// </summary>
        private async Task UpdateQueryableCriteria(string selectedObject)
        {
            var matchedColumns = await MatchColumnsForSelectedObject(selectedObject);

            Storage.Remove(x => x.Label == "Queryable Criteria");
            Storage.Add("Queryable Criteria", new KeyValueListCM(matchedColumns.ToArray()));
        }

        /// <summary>
        /// Add query builder widget to action.
        /// </summary>
        private void AddQueryBuilder()
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

            AddControl(queryBuilder);
        }

        #endregion Configuration.

        public Build_Query_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }
        public override async Task Run()
        {
            var sqlQueryCM = ExtractSelectedQueryFromCrate();
            if (sqlQueryCM == null)
            {
                RaiseError("Selected Query crate was not found in Action's CrateStorage");
                return;
            }
            var sqlQueryCrate = Fr8.Infrastructure.Data.Crates.Crate<StandardQueryCM>.FromContent("Sql Query", sqlQueryCM);
            Payload.Add(sqlQueryCrate);
            Success();
            await Task.Yield();
        }

        public override async Task Initialize()
        {
            var columnDefinitions = await ExtractColumnDefinitions();
            List<KeyValueDTO> tablesList = null;
            if (columnDefinitions != null)
            {
                tablesList = ExtractTableNames(columnDefinitions);
            }
            if (tablesList == null || tablesList.Count == 0)
            {
                AddLabelControl("UpstreamError","Unexpected error",
                    "No upstream crates found to extract table definitions.");
                return;
            }
            AddSelectObjectDdl();
            AddLabelControl("SelectObjectError", "No object selected", "Please select object from the list above.");
            Storage.RemoveByLabel("Available Tables");
            Storage.Add("Available Tables", new KeyValueListCM(tablesList));
            await Task.Yield();
        }

        public override async Task FollowUp()
        {
            RemoveControl<TextBlock>("SelectObjectError");

            var selectedObject = ExtractSelectedObjectFromControl();
            if (string.IsNullOrEmpty(selectedObject))
            {
                AddLabelControl("SelectObjectError","No object selected", "Please select object from the list above.");
                return;
            }

            var prevSelectedObject = this["Selected Object"];

            if (prevSelectedObject != selectedObject)
            {
                RemoveControl<QueryBuilder>("SelectedQuery");
                AddQueryBuilder();

                await UpdateQueryableCriteria(selectedObject);
            }
            
            this["Selected Object"] = selectedObject;

            UpdateSelectedQueryCrate();
        }
    }
}