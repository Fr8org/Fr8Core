using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Actions
{
    // The generic interface inheritance.
    public class Select_Fr8_Object_v1 : BasePluginAction
    {
        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public readonly DropDownListControlDefinitionDTO Selected_Fr8_Object;

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Selected_Fr8_Object = new DropDownListControlDefinitionDTO()
                {
                    Label = "Select Fr8 Object",
                    Name = "Selected_Fr8_Object",
                    Value = "",
                    Required = true,
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onChange", "requestConfig")
                    },
                    Source = new FieldSourceDTO
                    {
                        Label = "Select Fr8 Object",
                        ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME,
                    }
                };
                
                Controls.Add(Selected_Fr8_Object);
            }
        }

        // configure the action will return the initial UI crate 
        public async Task<ActionDTO> Configure(ActionDTO curActionDataPackageDTO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDTO, ConfigurationEvaluator);
        }

        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            var crateDesignTimeFields = PackFr8ObjectCrate();

            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(crateDesignTimeFields);
            }

            return Task.FromResult(curActionDTO);
        }

        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
            {
                var confControls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                
                if (confControls != null )
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

            return await Task.FromResult(curActionDTO);
        }
        
        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (Crate.IsEmptyStorage(curActionDTO.CrateStorage))
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
            var httpClient = new HttpClient();

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "manifests/"
                + Int32.Parse(fr8Object);
            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsAsync<CrateDTO>();
                
                return Crate.FromDto(content);
            }
        }

    }
}