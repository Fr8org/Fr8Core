using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
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
        private class ActionUi : StandardConfigurationControlsCM
        {
            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new DropDownList()
                {
                    Name = "SelectObjectDdl",
                    Label = "Search for",
                    Source = new FieldSourceDTO
                    {
                        Label = "AvailableObjects",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                    },
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                });
            }
        }


        public FindObjectHelper FindObjectHelper { get; set; }

        public FindObjects_Solution_v1()
        {
            FindObjectHelper = new FindObjectHelper();
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
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(PackAvailableObjects(connectionString));
            }

            return Task.FromResult(actionDO);
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DockyardDB"].ConnectionString;
        }

        private Crate PackAvailableObjects(string connectionString)
        {
            var tableDefinitions = FindObjectHelper.RetrieveTableDefinitions(connectionString);
            var tableDefinitionCrate =
                Crate.CreateDesignTimeFieldsCrate(
                    "AvailableObjects",
                    tableDefinitions.ToArray()
                );

            return tableDefinitionCrate;
        }

        #endregion Configration.
    }
}