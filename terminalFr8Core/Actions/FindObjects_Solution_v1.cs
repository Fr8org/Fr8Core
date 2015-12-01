using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Control;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

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
            using (var updater = Crate.UpdateStorage(actionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                // updater.CrateStorage.Add(PackAvailableTemplates(authTokenDO));
                // updater.CrateStorage.Add(PackAvailableEvents());
                // updater.CrateStorage.Add(await PackAvailableHandlers(actionDO));
            }

            return Task.FromResult(actionDO);
        }
    }
}