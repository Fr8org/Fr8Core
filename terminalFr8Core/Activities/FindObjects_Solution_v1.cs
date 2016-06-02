using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using terminalFr8Core.Infrastructure;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace terminalFr8Core.Activities
{
    public class FindObjects_Solution_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "FindObjects_Solution",
            Label = "Find Objects Solution",
            Category = ActivityCategory.Solution,
            Version = "1",
            MinPaneWidth = 330,
            Type = ActivityType.Solution,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        private const string SolutionName = "Find Objects Solution";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "terminalFr8Core";
        private const string SolutionBody = @"<p>This is the FindObjects Solution.</p>";
        public FindObjectHelper FindObjectHelper { get; set; }

        public FindObjects_Solution_v1(ICrateManager crateManager)
            : base(crateManager)
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configration.

        private string GetCurrentSelectedObject()
        {
            var selectObjectDdl = GetControl<DropDownList>("SelectObjectDdl");
            return selectObjectDdl?.Value;
        }

        private List<FilterConditionDTO> GetCurrentSelectedConditions()
        {
            var queryBuilder = GetControl<QueryBuilder>("QueryBuilder");
            if (queryBuilder?.Value == null)
            {
                return new List<FilterConditionDTO>();
            }
            var conditions = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                queryBuilder.Value
            );

            return conditions;
        }

        private bool NeedsCreateQueryBuilder()
        {
            var currentSelectedObject = GetCurrentSelectedObject();
            if (string.IsNullOrEmpty(currentSelectedObject))
            {
                return false;
            }
            if (GetControl<QueryBuilder>("QueryBuilder") == null)
            {
                return true;
            }
            return false;
        }

        private bool NeedsRemoveQueryBuilder()
        {
            var currentSelectedObject = GetCurrentSelectedObject();
            var prevSelectedValue = "";
            var prevSelectedObjectFields = Storage
                .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "PrevSelectedObject")
                .FirstOrDefault();

            if (prevSelectedObjectFields != null)
            {
                var prevSelectedObjectField = prevSelectedObjectFields.Fields
                    .FirstOrDefault(x => x.Key == "PrevSelectedObject");

                if (prevSelectedObjectField != null)
                {
                    prevSelectedValue = prevSelectedObjectField.Value;
                }
            }

            if (currentSelectedObject != prevSelectedValue)
            {
                if (GetControl<QueryBuilder>("QueryBuilder") != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddSelectObjectDdl()
        {
            AddControl(
                new DropDownList()
                {
                    Name = "SelectObjectDdl",
                    Label = "Search for",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableObjects",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                }
            );
        }

        private void AddAvailableObjects(string connectionString)
        {
            var tableDefinitions = FindObjectHelper.RetrieveTableDefinitions(connectionString);
            var tableDefinitionCrate = CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableObjects",
                    tableDefinitions.ToArray()
                );
            Storage.Add(tableDefinitionCrate);
        }

        private void UpdatePrevSelectedObject()
        {
            var currentSelectedObject = GetCurrentSelectedObject() ?? "";

            UpdateDesignTimeCrateValue("PrevSelectedObject",
                new FieldDTO("PrevSelectedObject", currentSelectedObject)
            );
        }

        private void AddQueryBuilder()
        {
            AddControl(
                new QueryBuilder()
                {
                    Name = "QueryBuilder",
                    Label = "Query",
                    Required = true,
                    Source = new FieldSourceDTO
                    {
                        Label = "Queryable Criteria",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    }
                }
            );
        }

        private void RemoveQueryBuilder()
        {
            RemoveControl<QueryBuilder>("QueryBuilder");
        }

        private void UpdateQueryableCriteriaCrate()
        {
            var supportedColumnTypes = new HashSet<DbType>()
            {
                DbType.String,
                DbType.Int32,
                DbType.Boolean
            };

            var currentSelectedObject = GetCurrentSelectedObject();

            var criteria = new List<FieldDTO>();
            if (!string.IsNullOrEmpty(currentSelectedObject))
            {
                // TODO: FR-2347, fix here.
                var columns = FindObjectHelper.MatchColumnsForSelectedObject(GetConnectionString(), currentSelectedObject);
                criteria.AddRange(columns);
            }

            UpdateDesignTimeCrateValue(
                "Queryable Criteria",
                criteria.ToArray()
            );
        }

        private void AddRunButton()
        {
            AddControl(
                new RunPlanButton()
                {
                    Name = "RunPlan",
                    Label = "Run Plan",
                }
            );
        }

        private void RemoveRunButton()
        {
            RemoveControl<RunPlanButton>("RunPlan");
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DockyardDB"].ConnectionString;
        }

        #endregion Configration.

        #region Child action management.

        private async Task<ActivityPayload> CreateConnectToSqlActivity()
        {
            var connectionString = GetConnectionString();
            var connectToSqlAT = await GetActivityTemplate("terminalFr8Core", "ConnectToSql");
            var connectToSqlActionDO = await AddAndConfigureChildActivity(ActivityId, connectToSqlAT);

            connectToSqlActionDO.CrateStorage = 
                    new CrateStorage()
                    {
                        Crate<FieldDescriptionsCM>.FromContent(
                            "Sql Connection String",
                            new FieldDescriptionsCM(
                                new FieldDTO(connectionString, connectionString)
                            )
                        ),
                        Crate<FieldDescriptionsCM>.FromContent(
                            "Sql Table Definitions",
                            new FieldDescriptionsCM(
                                FindObjectHelper.RetrieveColumnDefinitions(connectionString)
                            )
                        ),
                        Crate<FieldDescriptionsCM>.FromContent(
                            "Sql Column Types",
                            new FieldDescriptionsCM(
                                FindObjectHelper.RetrieveColumnTypes(connectionString)
                            )
                        )
                    };

            return connectToSqlActionDO;
        }

        private async Task<ActivityPayload> CreateBuildQueryActivity()
        {
            var selectedObject = GetCurrentSelectedObject();
            var selectedConditions = GetCurrentSelectedConditions();
            var buildQueryAT = await GetActivityTemplate("terminalFr8Core", "BuildQuery");
            var buildQueryActivityDO = await AddAndConfigureChildActivity(ActivityId, buildQueryAT);
            buildQueryActivityDO.CrateStorage = 
                    new CrateStorage()
                    {
                        Crate<StandardQueryCM>.FromContent(
                            "Selected Query",
                            new StandardQueryCM(
                                new QueryDTO(selectedObject, selectedConditions)
                            )
                        )
                    };
            
            return buildQueryActivityDO;
        }

        private async Task<ActivityPayload> CreateExecuteSqlActivity()
        {
            var executeSqlAT = await GetActivityTemplate("terminalFr8Core", "ExecuteSql");
            return await AddAndConfigureChildActivity(ActivityId, executeSqlAT);
        }

        private async Task UpdateChildActivities()
        {
            var connectToSqlActionDO = await CreateConnectToSqlActivity();
            if (GetControl<QueryBuilder>("QueryBuilder") != null)
            {
                var buildQueryActionDO = await CreateBuildQueryActivity();
                var executeSqlActionDO = await CreateExecuteSqlActivity();
            }
        }

        #endregion Child action management.

        #region Execution

        #endregion
        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        protected override Task<SolutionPageDTO> GetDocumentation(string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            return
                Task.FromResult(GenerateErrorResponse("Unknown displayMechanism: we currently support MainPage cases"));
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            var connectionString = GetConnectionString();
            Storage.Clear();
            AddSelectObjectDdl();
            AddAvailableObjects(connectionString);
            UpdatePrevSelectedObject();
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            if (NeedsRemoveQueryBuilder())
            {
                RemoveQueryBuilder();
                RemoveRunButton();
            }
            if (NeedsCreateQueryBuilder())
            {
                AddQueryBuilder();
                AddRunButton();
                UpdateQueryableCriteriaCrate();
            }
            UpdatePrevSelectedObject();
            await UpdateChildActivities();
        }
    }
}