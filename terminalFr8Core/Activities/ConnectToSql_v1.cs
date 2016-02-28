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
    public class ConnectToSql_v1 : BaseTerminalActivity
    {
        public FindObjectHelper FindObjectHelper { get; set; }

        public ConnectToSql_v1()
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configuration.

        public override ConfigurationRequestType ConfigurationEvaluator(
            ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(CreateControlsCrate());
            }

            return Task.FromResult<ActivityDO>(curActivityDO);
        }

        private Crate CreateControlsCrate()
        {
            var control = new TextBox()
            {
                Label = "SQL Connection String",
                Name = "ConnectionString",
                Required = true,
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };

            return PackControlsCrate(control);
        }

        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                RemoveControl(crateStorage, "ErrorLabel");


                crateStorage.RemoveByLabel("Sql Table Definitions");

            var connectionString = ExtractConnectionString(curActivityDO);
                
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var tableDefinitions = FindObjectHelper.RetrieveColumnDefinitions(connectionString);
                    var tableDefinitionCrate = 
                        CrateManager.CreateDesignTimeFieldsCrate(
                            "Sql Table Definitions",
                            tableDefinitions.ToArray()
                        );

                    var columnTypes = FindObjectHelper.RetrieveColumnTypes(connectionString);
                    var columnTypesCrate =
                        CrateManager.CreateDesignTimeFieldsCrate(
                            "Sql Column Types",
                            columnTypes.ToArray()
                        );

                    var connectionStringFieldList = new List<FieldDTO>()
                    {
                        new FieldDTO() { Key = connectionString, Value = connectionString }
                    };
                    var connectionStringCrate =
                        CrateManager.CreateDesignTimeFieldsCrate(
                            "Sql Connection String",
                            connectionStringFieldList.ToArray()
                        );

                        crateStorage.Add(tableDefinitionCrate);
                        crateStorage.Add(columnTypesCrate);
                        crateStorage.Add(connectionStringCrate);
                }
                catch
                {
                    AddLabelControl(
                            crateStorage,
                        "ErrorLabel",
                        "Unexpected error",
                        "Error occured while trying to fetch columns from database specified."
                    );
                }
            }
            }

            return base.FollowupConfigurationResponse(curActivityDO, authTokenDO);
        }

        private string ExtractConnectionString(ActivityDO curActivityDO)
        {
            var configControls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            var connectionStringControl = configControls.FindByName("ConnectionString");

            return connectionStringControl.Value;
        }

        #endregion Configuration.

        #region Execution.

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }

        #endregion Execution.
    }
}