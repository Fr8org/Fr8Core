using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using AutoMapper;
using Data.Control;
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
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class ConnectToSql_v1 : BaseTerminalAction
    {
        public FindObjectHelper FindObjectHelper { get; set; }

        public ConnectToSql_v1()
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(CreateControlsCrate());
            }

            return Task.FromResult<ActionDO>(curActionDO);
        }

        private Crate CreateControlsCrate()
        {
            var control = new TextBox()
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

        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                RemoveControl(updater.CrateStorage, "ErrorLabel");


                updater.CrateStorage.RemoveByLabel("Sql Table Definitions");

            var connectionString = ExtractConnectionString(curActionDO);
                
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var tableDefinitions = FindObjectHelper.RetrieveColumnDefinitions(connectionString);
                    var tableDefinitionCrate = 
                        Crate.CreateDesignTimeFieldsCrate(
                            "Sql Table Definitions",
                            tableDefinitions.ToArray()
                        );

                    var columnTypes = FindObjectHelper.RetrieveColumnTypes(connectionString);
                    var columnTypesCrate =
                        Crate.CreateDesignTimeFieldsCrate(
                            "Sql Column Types",
                            columnTypes.ToArray()
                        );

                    var connectionStringFieldList = new List<FieldDTO>()
                    {
                        new FieldDTO() { Key = connectionString, Value = connectionString }
                    };
                    var connectionStringCrate =
                        Crate.CreateDesignTimeFieldsCrate(
                            "Sql Connection String",
                            connectionStringFieldList.ToArray()
                        );

                    updater.CrateStorage.Add(tableDefinitionCrate);
                    updater.CrateStorage.Add(columnTypesCrate);
                    updater.CrateStorage.Add(connectionStringCrate);
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

            return base.FollowupConfigurationResponse(curActionDO, authTokenDO);
        }

        private string ExtractConnectionString(ActionDO curActionDO)
        {
            var configControls = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var connectionStringControl = configControls.FindByName("ConnectionString");

            return connectionStringControl.Value;
        }

        #endregion Configuration.

        #region Execution.

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }

        #endregion Execution.
    }
}