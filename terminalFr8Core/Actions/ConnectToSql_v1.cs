using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using TerminalSqlUtilities;

namespace terminalFr8Core.Actions
{
    public class ConnectToSql_v1 : BasePluginAction
    {
        #region Configuration.

        private const string DefaultDbProvider = "System.Data.SqlClient";


        public override ConfigurationRequestType ConfigurationEvaluator(
            ActionDO curActionDO)
        {
            if (curActionDO.CrateStorageDTO() == null
                || curActionDO.CrateStorageDTO().CrateDTO == null
                || curActionDO.CrateStorageDTO().CrateDTO.Count == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            if (curActionDO.CrateStorageDTO() == null)
            {
                curActionDO.UpdateCrateStorageDTO(new List<CrateDTO>());
            }

            var crateControls = CreateControlsCrate();
            var curCrateDTOList = new List<CrateDTO>();
            curCrateDTOList.Add(crateControls);
            curActionDO.UpdateCrateStorageDTO(curCrateDTOList);

            return Task.FromResult<ActionDO>(curActionDO);
        }

        private CrateDTO CreateControlsCrate()
        {
            var control = new TextBoxControlDefinitionDTO()
            {
                Label = "SQL Connection String",
                Name = "ConnectionString",
                Required = true,
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                }
            };

            return PackControlsCrate(control);
        }

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            RemoveControl(curActionDO, "ErrorLabel");

            Crate.RemoveCrateByLabel(
                curActionDO.CrateStorageDTO().CrateDTO,
                "Sql Table Definitions"
            );

            var connectionString = ExtractConnectionString(curActionDO);
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var tableDefinitions = RetrieveTableDefinitions(connectionString);
                    var tableDefinitionCrate = 
                        Crate.CreateDesignTimeFieldsCrate(
                            "Sql Table Definitions",
                            tableDefinitions.ToArray()
                        );

                    var columnTypes = RetrieveColumnTypes(connectionString);
                    var columnTypesCrate =
                        Crate.CreateDesignTimeFieldsCrate(
                            "Sql Column Types",
                            columnTypes.ToArray()
                        );
                    var curCrateDTOList = new List<CrateDTO>();
                    curCrateDTOList.Add(tableDefinitionCrate);
                    curCrateDTOList.Add(columnTypesCrate);
                    curActionDO.UpdateCrateStorageDTO(curCrateDTOList);

                }
                catch
                {
                    AddLabelControl(
                        curActionDO,
                        "ErrorLabel",
                        "Unexpected error",
                        "Error occured while trying to fetch columns from database specified."
                    );
                }
            }

            return base.FollowupConfigurationResponse(curActionDO, authTokenDO);
        }

        private string ExtractConnectionString(ActionDO curActionDO)
        {
            var configControls = Crate.GetConfigurationControls(Mapper.Map<ActionDO>(curActionDO));
            var connectionStringControl = configControls.FindByName("ConnectionString");

            return connectionStringControl.Value;
        }

        private void ListAllDbColumns(string connectionString, Action<IEnumerable<ColumnInfo>> callback)
        {
            var provider = DbProvider.GetDbProvider(DefaultDbProvider);

            using (var conn = provider.CreateConnection(connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    var columns = provider.ListAllColumns(tx);

                    if (callback != null)
                    {
                        callback.Invoke(columns);
                    }
                }
            }
        }

        private List<FieldDTO> RetrieveTableDefinitions(string connectionString)
        {
            var fieldsList = new List<FieldDTO>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    var fullColumnName = GetColumnName(column);

                    fieldsList.Add(new FieldDTO()
                    {
                        Key = fullColumnName,
                        Value = fullColumnName
                    });
                }
            });

            return fieldsList;
        }

        private List<FieldDTO> RetrieveColumnTypes(string connectionString)
        {
            var fieldsList = new List<FieldDTO>();

            ListAllDbColumns(connectionString, columns =>
            {
                foreach (var column in columns)
                {
                    var fullColumnName = GetColumnName(column);

                    fieldsList.Add(new FieldDTO()
                    {
                        Key = fullColumnName,
                        Value = column.DbType.ToString()
                    });
                }
            });

            return fieldsList;
        }

        private string GetColumnName(ColumnInfo columnInfo)
        {
            if (string.IsNullOrEmpty(columnInfo.TableInfo.SchemaName))
            {
                return string.Format(
                    "{0}.{1}",
                    columnInfo.TableInfo.TableName,
                    columnInfo.ColumnName
                );
            }
            else
            {
                return string.Format(
                    "{0}.{1}.{2}",
                    columnInfo.TableInfo.SchemaName,
                    columnInfo.TableInfo.TableName,
                    columnInfo.ColumnName
                );
            }
        }

        #endregion Configuration.

        #region Execution.

        public Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO = null)
        {
            return Task.FromResult<PayloadDTO>(null);
        }

        #endregion Execution.
    }
}