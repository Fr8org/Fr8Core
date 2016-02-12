using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalSqlUtilities;
using Utilities.Configuration.Azure;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class FindObjects_Solution_v1 : BaseTerminalActivity
    {
        public FindObjectHelper FindObjectHelper { get; set; }

        public FindObjects_Solution_v1()
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configration.

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(
            ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            var connectionString = GetConnectionString();

            using (var updater = Crate.UpdateStorage(activityDO))
            {
                updater.CrateStorage.Clear();

                AddSelectObjectDdl(updater);
                AddAvailableObjects(updater, connectionString);

                UpdatePrevSelectedObject(updater);
            }

            return Task.FromResult(activityDO);
        }

        protected async override Task<ActivityDO> FollowupConfigurationResponse(
            ActivityDO activityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(activityDO))
            {
                var crateStorage = updater.CrateStorage;

                if (NeedsRemoveQueryBuilder(crateStorage))
                {
                    RemoveQueryBuilder(updater);
                    RemoveRunButton(updater);
                }

                if (NeedsCreateQueryBuilder(crateStorage))
                {
                    AddQueryBuilder(updater);
                    AddRunButton(updater);
                    UpdateQueryableCriteriaCrate(updater);
                }

                UpdatePrevSelectedObject(updater);
            }

            await UpdateChildActions(activityDO);

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
                .CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "PrevSelectedObject")
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

        private void AddSelectObjectDdl(ICrateStorageUpdater updater)
        {
            AddControl(
                updater.CrateStorage,
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

        private void AddAvailableObjects(ICrateStorageUpdater updater, string connectionString)
        {
            var tableDefinitions = FindObjectHelper.RetrieveTableDefinitions(connectionString);
            var tableDefinitionCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableObjects",
                    tableDefinitions.ToArray()
                );

            updater.CrateStorage.Add(tableDefinitionCrate);
        }

        private void UpdatePrevSelectedObject(ICrateStorageUpdater updater)
        {
            var currentSelectedObject = GetCurrentSelectedObject(updater.CrateStorage) ?? "";

            UpdateDesignTimeCrateValue(
                updater.CrateStorage,
                "PrevSelectedObject",
                new FieldDTO("PrevSelectedObject", currentSelectedObject)
            );
        }

        private void AddQueryBuilder(ICrateStorageUpdater updater)
        {
            AddControl(
                updater.CrateStorage,
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

        private void RemoveQueryBuilder(ICrateStorageUpdater updater)
        {
            RemoveControl(updater.CrateStorage, "QueryBuilder");
        }

        private void UpdateQueryableCriteriaCrate(ICrateStorageUpdater updater)
        {
            var supportedColumnTypes = new HashSet<DbType>()
            {
                DbType.String,
                DbType.Int32,
                DbType.Boolean
            };

            var currentSelectedObject = GetCurrentSelectedObject(updater.CrateStorage);

            var criteria = new List<FieldDTO>();
            if (!string.IsNullOrEmpty(currentSelectedObject))
            {
                var columns = FindObjectHelper.MatchColumnsForSelectedObject(GetConnectionString(), currentSelectedObject);
                criteria.AddRange(columns);
            }

            UpdateDesignTimeCrateValue(
                updater.CrateStorage,
                "Queryable Criteria",
                criteria.ToArray()
            );
        }

        private void AddRunButton(ICrateStorageUpdater updater)
        {
            AddControl(
                updater.CrateStorage,
                new RunRouteButton()
                {
                    Name = "RunRoute",
                    Label = "Run Plan",
                }
            );
        }

        private void RemoveRunButton(ICrateStorageUpdater updater)
        {
            RemoveControl(updater.CrateStorage, "RunRoute");
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
            var connectToSqlActionDO = await AddAndConfigureChildActivity(activityDO, "ConnectToSql");

            connectToSqlActionDO.CrateStorage = Crate.CrateStorageAsStr(
                    new CrateStorage()
                    {
                        Crate<StandardDesignTimeFieldsCM>.FromContent(
                            "Sql Connection String",
                            new StandardDesignTimeFieldsCM(
                                new FieldDTO(connectionString, connectionString)
                            )
                        ),
                        Crate<StandardDesignTimeFieldsCM>.FromContent(
                            "Sql Table Definitions",
                            new StandardDesignTimeFieldsCM(
                                FindObjectHelper.RetrieveColumnDefinitions(connectionString)
                            )
                        ),
                        Crate<StandardDesignTimeFieldsCM>.FromContent(
                            "Sql Column Types",
                            new StandardDesignTimeFieldsCM(
                                FindObjectHelper.RetrieveColumnTypes(connectionString)
                            )
                        )
                    });

            return connectToSqlActionDO;
        }

        private async Task<ActivityDO> CreateBuildQueryActivity(ActivityDO activityDO)
        {
            var crateStorage = Crate.GetStorage(activityDO);
            var selectedObject = GetCurrentSelectedObject(crateStorage);
            var selectedConditions = GetCurrentSelectedConditions(crateStorage);

            var buildQueryActivityDO = await AddAndConfigureChildActivity(activityDO, "BuildQuery");
            buildQueryActivityDO.CrateStorage = Crate.CrateStorageAsStr(
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

        private async Task<ActivityDO> CreateExecuteSqlAction(ActivityDO activityDO)
        {
            return await AddAndConfigureChildActivity(activityDO, "ExecuteSql");
        }

        private async Task UpdateChildActions(ActivityDO activityDO)
        {
            var connectToSqlActionDO = await CreateConnectToSqlActivity(activityDO);

            if (FindControl(Crate.GetStorage(activityDO), "QueryBuilder") != null)
            {
                var buildQueryActionDO = await CreateBuildQueryActivity(activityDO);

                var executeSqlActionDO = await CreateExecuteSqlAction(activityDO);
            }
        }

        #endregion Child action management.

        #region Execution

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }

        #endregion
    }
}