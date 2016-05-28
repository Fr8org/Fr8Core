using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Fr8Infrastructure.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Activities
{
    // The generic interface inheritance.
    public class Select_Fr8_Object_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Select_Fr8_Object",
            Label = "Select Fr8 Object",
            Category = ActivityCategory.Processors,
            Version = "1",
            MinPaneWidth = 330,
            Tags = Tags.Internal,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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
                + int.Parse(fr8Object);
            var response = await client.GetAsync<CrateDTO>(new Uri(url));
            return CrateManager.FromDto(response);
		}

        public Select_Fr8_Object_v1(ICrateManager crateManager)
            : base(false, crateManager)
        {
        }

        public override Task Run()
        {
            Success();
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            var crateDesignTimeFields = PackFr8ObjectCrate();
            Storage.Clear();
            Storage.Add(PackControls(new ActivityUi()));
            Storage.Add(crateDesignTimeFields);
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            var actionUi = new ActivityUi();
            // Clone properties of StandardConfigurationControlsCM to handy ActionUi
            actionUi.ClonePropertiesFrom(ConfigurationControls);

            if (!string.IsNullOrWhiteSpace(actionUi.Selected_Fr8_Object.Value))
            {
                var fr8ObjectCrateDTO = await GetDesignTimeFieldsCrateOfSelectedFr8Object(actionUi.Selected_Fr8_Object.Value);

                const string designTimeControlName = "Select Fr8 Object Properties";
                actionUi.Selected_Fr8_Object.Label = designTimeControlName;

                // Sync changes from ActionUi to StandardConfigurationControlsCM
                ConfigurationControls.ClonePropertiesFrom(actionUi);

                Storage.RemoveByLabel(designTimeControlName);
                Storage.Add(fr8ObjectCrateDTO);
            }
        }
    }
}