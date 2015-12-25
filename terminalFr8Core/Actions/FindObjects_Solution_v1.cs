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
    public class FindObjects_Solution_v1 : BaseTerminalAction
    {
        public FindObjectHelper FindObjectHelper { get; set; }
        public ExplicitConfigurationHelper ExplicitConfigurationHelper { get; set; }

        public FindObjects_Solution_v1()
        {
            FindObjectHelper = new FindObjectHelper();
            ExplicitConfigurationHelper = new ExplicitConfigurationHelper();
        }

        #region Configration.

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDO> InitialConfigurationResponse(
            ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            var connectionString = GetConnectionString();

            using (var updater = Crate.UpdateStorage(actionDO))
            {
                updater.CrateStorage.Clear();

                AddSelectObjectDdl(updater);
                AddAvailableObjects(updater, connectionString);

                UpdatePrevSelectedObject(updater);
            }

            return Task.FromResult(actionDO);
        }

        protected async override Task<ActionDO> FollowupConfigurationResponse(
            ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(actionDO))
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

            await UpdateChildActions(actionDO);

            return actionDO;
        }

        private string GetCurrentSelectedObject(CrateStorage storage)
        {
            var selectObjectDdl = FindControl(storage, "SelectObjectDdl") as DropDownList;
            if (selectObjectDdl == null)
            {
                return null;
            }

            return selectObjectDdl.Value;
        }

        private List<FilterConditionDTO> GetCurrentSelectedConditions(CrateStorage storage)
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

        private bool NeedsCreateQueryBuilder(CrateStorage storage)
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

        private bool NeedsRemoveQueryBuilder(CrateStorage storage)
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
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
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
                    Label = "Run Route",
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

        private async Task<ActivityTemplateDTO> ExtractActivityTemplate(ActionDO actionDO, string name)
        {
            var activityTemplate =
                (await HubCommunicator.GetActivityTemplates(actionDO))
                .FirstOrDefault(x => x.Name == name);

            return activityTemplate;
        }

        private async Task<ActionDO> CreateConnectToSqlAction(ActionDO actionDO)
        {
            var connectionString = GetConnectionString();

            var activityTemplateName = "ConnectToSql";
            var activityTemplateDTO = await ExtractActivityTemplate(actionDO, activityTemplateName);

            if (activityTemplateDTO == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            var connectToSqlActionDO = new ActionDO()
            {
                IsTempId = true,
                ActivityTemplateId = activityTemplateDTO.Id,
                CrateStorage = Crate.CrateStorageAsStr(
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
                    }
                ),
                CreateDate = DateTime.Now,
                Ordering = 1,
                Name = "Connect To Sql",
                Label = "Connect To Sql"
            };

            return connectToSqlActionDO;
        }

        private async Task<ActionDO> CreateBuildQueryAction(ActionDO actionDO)
        {
            var crateStorage = Crate.GetStorage(actionDO);
            var selectedObject = GetCurrentSelectedObject(crateStorage);
            var selectedConditions = GetCurrentSelectedConditions(crateStorage);

            var activityTemplateName = "BuildQuery";
            var activityTemplateDTO = await ExtractActivityTemplate(actionDO, activityTemplateName);

            if (activityTemplateDTO == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            var buildQueryActionDO = new ActionDO()
            {
                IsTempId = true,
                ActivityTemplateId = activityTemplateDTO.Id,
                CrateStorage = Crate.CrateStorageAsStr(
                    new CrateStorage()
                    {
                        Crate<StandardQueryCM>.FromContent(
                            "Selected Query",
                            new StandardQueryCM(
                                new QueryDTO(selectedObject, selectedConditions)
                            )
                        )
                    }
                ),
                CreateDate = DateTime.Now,
                Ordering = 2,
                Name = "Build Query",
                Label = "Build Query"
            };

            return buildQueryActionDO;
        }

        private async Task<ActionDO> CreateExecuteSqlAction(ActionDO actionDO)
        {
            var activityTemplateName = "ExecuteSql";
            var activityTemplateDTO = await ExtractActivityTemplate(actionDO, activityTemplateName);

            if (activityTemplateDTO == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            var executeSqlActionDO = new ActionDO()
            {
                IsTempId = true,
                ActivityTemplateId = activityTemplateDTO.Id,
                CrateStorage = Crate.EmptyStorageAsStr(),
                CreateDate = DateTime.Now,
                Ordering = 3,
                Name = "Execute Sql",
                Label = "Execute Sql"
            };

            return executeSqlActionDO;
        }

        private async Task UpdateChildActions(ActionDO actionDO)
        {
            actionDO.ChildNodes = new List<RouteNodeDO>();

            var connectToSqlActionDO = await CreateConnectToSqlAction(actionDO);
            actionDO.ChildNodes.Add(connectToSqlActionDO);

            if (FindControl(Crate.GetStorage(actionDO), "QueryBuilder") != null)
            {
                var buildQueryActionDO = await CreateBuildQueryAction(actionDO);
                actionDO.ChildNodes.Add(buildQueryActionDO);

                var executeSqlActionDO = await CreateExecuteSqlAction(actionDO);
                actionDO.ChildNodes.Add(executeSqlActionDO);
            }
        }

        #endregion Child action management.

        #region Execution

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetProcessPayload(curActionDO, containerId));
        }

        #endregion
    }
}