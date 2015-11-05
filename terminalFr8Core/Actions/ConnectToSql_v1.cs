using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using AutoMapper;
using Data.Crates;
using Newtonsoft.Json;
using Data.Entities;
using Hub.Managers;
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


        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (Crate.IsEmptyStorage(curActionDTO.CrateStorage))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {

            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(CreateControlsCrate());
            }

            return Task.FromResult<ActionDTO>(curActionDTO);
        }

        private Crate CreateControlsCrate()
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

        protected override Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                RemoveControl(updater.CrateStorage, "ErrorLabel");


                updater.CrateStorage.RemoveByLabel("Sql Table Definitions");

                var connectionString = ExtractConnectionString(curActionDTO);
                
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

                        updater.CrateStorage.Add(tableDefinitionCrate);
                        updater.CrateStorage.Add(columnTypesCrate);
                    }
                    catch
                    {
                        AddLabelControl(
                            updater.CrateStorage,
                            "ErrorLabel",
                            "Unexpected error",
                            "Error occured while trying to fetch columns from database specified."
                            );
                    }
                }
            }

            return base.FollowupConfigurationResponse(curActionDTO);
        }

        private string ExtractConnectionString(ActionDTO curActionDTO)
        {
            var configControls = Crate.GetStorage(curActionDTO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
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

        public Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            return Task.FromResult<PayloadDTO>(null);
        }

        #endregion Execution.
    }
}