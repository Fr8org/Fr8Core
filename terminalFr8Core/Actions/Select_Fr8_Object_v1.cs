using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using Data.Entities;
using StructureMap;
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace terminalFr8Core.Actions
{
    // The generic interface inheritance.
    public class Select_Fr8_Object_v1 : BaseTerminalAction
    {
        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public readonly DropDownList Selected_Fr8_Object;

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Selected_Fr8_Object = new DropDownList()
                {
                    Label = "Select Fr8 Object",
                    Name = "Selected_Fr8_Object",
                    Value = "",
                    Required = true,
                    Events = new List<ControlEvent>(){ ControlEvent.RequestConfig },
                    Source = new FieldSourceDTO
                    {
                        Label = "Select Fr8 Object",
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    }
                };

                Controls.Add(Selected_Fr8_Object);
            }
        }

        // configure the action will return the initial UI crate 
        public override async Task<ActionDO> Configure(ActionDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var crateDesignTimeFields = PackFr8ObjectCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(crateDesignTimeFields);
            }

            return Task.FromResult(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                var confControls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (confControls != null)
                {
                    var ui = new ActionUi();

                    // Clone properties of StandardConfigurationControlsCM to handy ActionUi
                    ui.ClonePropertiesFrom(confControls);

                    if (!string.IsNullOrWhiteSpace(ui.Selected_Fr8_Object.Value))
                    {
                        var fr8ObjectCrateDTO = await GetDesignTimeFieldsCrateOfSelectedFr8Object(ui.Selected_Fr8_Object.Value);

                        const string designTimeControlName = "Select Fr8 Object Properties";
                        ui.Selected_Fr8_Object.Label = designTimeControlName;

                        // Sync changes from ActionUi to StandardConfigurationControlsCM
                        confControls.ClonePropertiesFrom(ui);

                        updater.CrateStorage.RemoveByLabel(designTimeControlName);
                        updater.CrateStorage.Add(fr8ObjectCrateDTO);
                    }
                }
            }

            return await Task.FromResult(curActionDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        private Crate PackFr8ObjectCrate()
        {
            var fields = new List<FieldDTO> {
                    new FieldDTO(){
                       Key = "Routes",
                       Value = "19"
                   },
                   new FieldDTO(){
                       Key = "Containers",
                       Value = "21"
                   }
            }.ToArray();

            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Select Fr8 Object",
                fields);
            return createDesignTimeFields;
        }

        // Get the Design time fields crate.
        private async Task<Crate> GetDesignTimeFieldsCrateOfSelectedFr8Object(string fr8Object)
        {
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/manifests?id="
                + Int32.Parse(fr8Object);
            var response = await client.GetAsync<CrateDTO>(new Uri(url));
            return Crate.FromDto(response);
		}

		#region Execution
		public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
	    {
			return Success(await GetPayload(actionDO, containerId));
	    }
		#endregion
	}
}