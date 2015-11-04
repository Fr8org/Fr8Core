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

        protected override Task<ActionDO> InitialConfigurationResponse(
            ActionDO curActionDO)
        {
            if (curActionDO.CrateStorage == null)
            {
                curActionDO.UpdateCrateStorageDTO(new CrateStorageDTO().CrateDTO);
            }

            var crateControls = CreateControlsCrate();
            curActionDO.CrateStorageDTO().CrateDTO.Add(crateControls);

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

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO)
        {
            RemoveErrorControl(curActionDO);

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
                    var tableDefinitionCrate = Crate
                        .CreateDesignTimeFieldsCrate(
                            "Sql Table Definitions",
                            tableDefinitions.ToArray()
                        );

                    curActionDO.CrateStorageDTO().CrateDTO.Add(tableDefinitionCrate);
                }
                catch
                {
                    AddErrorControl(curActionDO);
                }
            }

            return base.FollowupConfigurationResponse(curActionDO);
        }

        private string ExtractConnectionString(ActionDO curActionDO)
        {
            var configControls = Crate.GetConfigurationControls(curActionDO);
            var connectionStringControl = configControls.FindByName("ConnectionString");

            return connectionStringControl.Value;
        }

        private List<FieldDTO> RetrieveTableDefinitions(string connectionString)
        {
            var provider = DbProvider.GetDbProvider(DefaultDbProvider);

            using (var conn = provider.CreateConnection(connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    var fieldsList = new List<FieldDTO>();

                    var columns = provider.ListAllColumns(tx);
                    foreach (var column in columns)
                    {
                        var fullColumnName = GetColumnName(column);

                        fieldsList.Add(new FieldDTO()
                        {
                            Key = fullColumnName,
                            Value = fullColumnName
                        });
                    }

                    return fieldsList;
                }
            }
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

        private void AddErrorControl(ActionDO curActionDO)
        {
            var controlsCrate = curActionDO.CrateStorageDTO().CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);

            if (controlsCrate == null) { return; }

            var controls = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                controlsCrate.Contents);

            if (controls == null) { return; }

            controls.Controls.Add(new TextBlockControlDefinitionDTO()
            {
                Label = "ErrorLabel",
                Value = "Error occured while trying to fetch columns from database specified.",
                CssClass = "well well-lg"
            });

            controlsCrate.Contents = JsonConvert.SerializeObject(controlsCrate);
        }

        private void RemoveErrorControl(ActionDO curActionDO)
        {
            var controlsCrate = curActionDO.CrateStorageDTO().CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME);

            if (controlsCrate == null) { return; }

            var controls = JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(
                controlsCrate.Contents);

            if (controls == null) { return; }

            
            var errorLabel = controls.Controls
                .FirstOrDefault(x => x.Label == "ErrorLabel");

            if (errorLabel != null)
            {
                controls.Controls.Remove(errorLabel);
                controlsCrate.Contents = JsonConvert.SerializeObject(controlsCrate);
            }
        }

        #endregion Configuration.

        #region Execution.

        public Task<PayloadDTO> Run(ActionDO curActionDO)
        {
            throw new NotImplementedException();
        }

        #endregion Execution.
    }
}