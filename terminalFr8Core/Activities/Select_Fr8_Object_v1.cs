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

namespace terminalFr8Core.Activities
{
    // The generic interface inheritance.
    public class Select_Fr8_Object_v1 : BaseTerminalActivity
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public readonly DropDownList Selected_Fr8_Object;

            public ActivityUi()
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
        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var crateDesignTimeFields = PackFr8ObjectCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(crateDesignTimeFields);
            }

            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configurationControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (configurationControls != null)
                {
                    var actionUi = new ActivityUi();

                    // Clone properties of StandardConfigurationControlsCM to handy ActionUi
                    actionUi.ClonePropertiesFrom(configurationControls);

                    if (!string.IsNullOrWhiteSpace(actionUi.Selected_Fr8_Object.Value))
                    {
                        var fr8ObjectCrateDTO = await GetDesignTimeFieldsCrateOfSelectedFr8Object(actionUi.Selected_Fr8_Object.Value);

                        const string designTimeControlName = "Select Fr8 Object Properties";
                        actionUi.Selected_Fr8_Object.Label = designTimeControlName;

                        // Sync changes from ActionUi to StandardConfigurationControlsCM
                        configurationControls.ClonePropertiesFrom(actionUi);

                        crateStorage.RemoveByLabel(designTimeControlName);
                        crateStorage.Add(fr8ObjectCrateDTO);
                    }
                }
            }

            return await Task.FromResult(curActivityDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        private Crate PackFr8ObjectCrate()
        {
            var fields = new List<FieldDTO> {
                    new FieldDTO(){
                       Key = "Plans",
                       Value = "19"
                   },
                   new FieldDTO(){
                       Key = "Containers",
                       Value = "21"
                   }
            }.ToArray();

            var createDesignTimeFields = CrateManager.CreateDesignTimeFieldsCrate(
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
            return CrateManager.FromDto(response);
		}

		#region Execution
		public async Task<PayloadDTO> Run(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
	    {
			return Success(await GetPayload(activityDO, containerId));
	    }
		#endregion
	}
}