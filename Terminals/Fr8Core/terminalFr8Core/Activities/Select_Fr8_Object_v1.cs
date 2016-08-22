using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;
using StructureMap;

namespace terminalFr8Core.Activities
{
    // The generic interface inheritance.
    public class Select_Fr8_Object_v1 : ExplicitTerminalActivity
    {
        private readonly IRestfulServiceClient _restfulServiceClient;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("6238483f-2cef-418e-bd7e-a52ddb1e01e5"),
            Name = "Select_Fr8_Object",
            Label = "Select Fr8 Object",
            Version = "1",
            MinPaneWidth = 330,
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
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
            var fields = new List<KeyValueDTO> {
                    new KeyValueDTO(){
                       Key = "Plans",
                       Value = "19"
                   },
                   new KeyValueDTO(){
                       Key = "Containers",
                       Value = "21"
                   }
            }.ToArray();

            var createDesignTimeFields = Crate.FromContent("Select Fr8 Object", new KeyValueListCM(fields));

            return createDesignTimeFields;
        }

        // Get the Design time fields crate.
        private async Task<Crate> GetDesignTimeFieldsCrateOfSelectedFr8Object(string fr8Object)
        {
            var url = CloudConfigurationManager.GetSetting("DefaultHubUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/manifests?id="
                + int.Parse(fr8Object);
            var response = await _restfulServiceClient.GetAsync<CrateDTO>(new Uri(url));

            return CrateStorageSerializer.Default.ConvertFromDto(response);
		}

        public Select_Fr8_Object_v1(ICrateManager crateManager, IRestfulServiceClient restfulServiceClient)
            : base(crateManager)
        {
            _restfulServiceClient = restfulServiceClient;
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

            AddControls(new ActivityUi().Controls);
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