using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class FindObjects_Solution_v1 : BaseTerminalActivity
    {
        private const string SolutionName = "Find Objects Solution";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "terminalFr8Core";
        private const string SolutionBody = @"<p>This is the FindObjects Solution.</p>";
        public FindObjectHelper FindObjectHelper { get; set; }

        public FindObjects_Solution_v1()
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configration.

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(
            ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            var connectionString = GetConnectionString();

            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                crateStorage.Clear();

                AddSelectObjectDdl(crateStorage);
                AddAvailableObjects(crateStorage, connectionString);

                UpdatePrevSelectedObject(crateStorage);
            }

            return Task.FromResult(activityDO);
        }

        protected async override Task<ActivityDO> FollowupConfigurationResponse(
            ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDO))
            {
                if (NeedsRemoveQueryBuilder(crateStorage))
                {
                    RemoveQueryBuilder(crateStorage);
                    RemoveRunButton(crateStorage);
                }

                if (NeedsCreateQueryBuilder(crateStorage))
                {
                    AddQueryBuilder(crateStorage);
                    AddRunButton(crateStorage);
                    UpdateQueryableCriteriaCrate(crateStorage);
                }

                UpdatePrevSelectedObject(crateStorage);
            }

            await UpdateChildActivities(activityDO);

            return activityDO;
        }

        private string GetCurrentSelectedObject(ICrateStorage storage)
        {
            var selectObjectDdl = FindControl(storage, "SelectObjectDdl") as DropDownList;
            if (selectObjectDdl == null)
            {
                return null;
            }

            return selectObjectDdl.Value;
        }

        private List<FilterConditionDTO> GetCurrentSelectedConditions(ICrateStorage storage)
        {
            var queryBuilder = FindControl(storage, "QueryBuilder");
            if (queryBuilder == null || queryBuilder.Value == null)
            {
                return new List<FilterConditionDTO>();
            }

            var conditions = JsonConvert.DeserializeObject<List<FilterConditionDTO>>(
                queryBuilder.Value
            );

            return conditions;
        }

        private bool NeedsCreateQueryBuilder(ICrateStorage storage)
        {
            var currentSelectedObject = GetCurrentSelectedObject(storage);

            if (string.IsNullOrEmpty(currentSelectedObject))
            {
                return false;
            }

            if (FindControl(storage, "QueryBuilder") == null)
            {
                return true;
            }

            return false;
        }

        private bool NeedsRemoveQueryBuilder(ICrateStorage storage)
        {
            var currentSelectedObject = GetCurrentSelectedObject(storage);

            var prevSelectedValue = "";

            var prevSelectedObjectFields = storage
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
                if (FindControl(storage, "QueryBuilder") != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddSelectObjectDdl(IUpdatableCrateStorage updater)
        {
            AddControl(
                updater,
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

        private void AddAvailableObjects(IUpdatableCrateStorage updater, string connectionString)
        {
            var tableDefinitions = FindObjectHelper.RetrieveTableDefinitions(connectionString);
            var tableDefinitionCrate =
                CrateManager.CreateDesignTimeFieldsCrate(
                    "AvailableObjects",
                    tableDefinitions.ToArray()
                );

            updater.Add(tableDefinitionCrate);
        }

        private void UpdatePrevSelectedObject(IUpdatableCrateStorage updater)
        {
            var currentSelectedObject = GetCurrentSelectedObject(updater) ?? "";

            UpdateDesignTimeCrateValue(
                updater,
                "PrevSelectedObject",
                new FieldDTO("PrevSelectedObject", currentSelectedObject)
            );
        }

        private void AddQueryBuilder(IUpdatableCrateStorage updater)
        {
            AddControl(
                updater,
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

        private void RemoveQueryBuilder(IUpdatableCrateStorage updater)
        {
            RemoveControl(updater, "QueryBuilder");
        }

        private void UpdateQueryableCriteriaCrate(IUpdatableCrateStorage updater)
        {
            var supportedColumnTypes = new HashSet<DbType>()
            {
                DbType.String,
                DbType.Int32,
                DbType.Boolean
            };

            var currentSelectedObject = GetCurrentSelectedObject(updater);

            var criteria = new List<FieldDTO>();
            if (!string.IsNullOrEmpty(currentSelectedObject))
            {
                // TODO: FR-2347, fix here.
                var columns = FindObjectHelper.MatchColumnsForSelectedObject(GetConnectionString(), currentSelectedObject);
                criteria.AddRange(columns);
            }

            UpdateDesignTimeCrateValue(
                updater,
                "Queryable Criteria",
                criteria.ToArray()
            );
        }

        private void AddRunButton(IUpdatableCrateStorage updater)
        {
            AddControl(
                updater,
                new RunPlanButton()
                {
                    Name = "RunPlan",
                    Label = "Run Plan",
                }
            );
        }

        private void RemoveRunButton(IUpdatableCrateStorage updater)
        {
            RemoveControl(updater, "RunPlan");
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DockyardDB"].ConnectionString;
        }

        #endregion Configration.

        #region Child action management.

        private async Task<ActivityDO> CreateConnectToSqlActivity(ActivityDO activityDO)
        {
            var connectionString = GetConnectionString();
            var connectToSqlAT = await GetActivityTemplate("terminalFr8Core", "ConnectToSql");
            var connectToSqlActionDO = await AddAndConfigureChildActivity(activityDO, connectToSqlAT);

            connectToSqlActionDO.CrateStorage = CrateManager.CrateStorageAsStr(
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
                    });

            return connectToSqlActionDO;
        }

        private async Task<ActivityDO> CreateBuildQueryActivity(ActivityDO activityDO)
        {
            var crateStorage = CrateManager.GetStorage(activityDO);
            var selectedObject = GetCurrentSelectedObject(crateStorage);
            var selectedConditions = GetCurrentSelectedConditions(crateStorage);
            var buildQueryAT = await GetActivityTemplate("terminalFr8Core", "BuildQuery");
            var buildQueryActivityDO = await AddAndConfigureChildActivity(activityDO, buildQueryAT);
            buildQueryActivityDO.CrateStorage = CrateManager.CrateStorageAsStr(
                    new CrateStorage()
                    {
                        Crate<StandardQueryCM>.FromContent(
                            "Selected Query",
                            new StandardQueryCM(
                                new QueryDTO(selectedObject, selectedConditions)
                            )
                        )
                    }
                );
            
            return buildQueryActivityDO;
        }

        private async Task<ActivityDO> CreateExecuteSqlActivity(ActivityDO activityDO)
        {
            var executeSqlAT = await GetActivityTemplate("terminalFr8Core", "ExecuteSql");
            return await AddAndConfigureChildActivity(activityDO, executeSqlAT);
        }

        private async Task UpdateChildActivities(ActivityDO activityDO)
        {
            var connectToSqlActionDO = await CreateConnectToSqlActivity(activityDO);

            if (FindControl(CrateManager.GetStorage(activityDO), "QueryBuilder") != null)
            {
                var buildQueryActionDO = await CreateBuildQueryActivity(activityDO);

                var executeSqlActionDO = await CreateExecuteSqlActivity(activityDO);
            }
        }

        #endregion Child action management.

        #region Execution

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }

        #endregion
        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonse("Unknown displayMechanism: we currently support MainPage cases"));
        }
    }
}