
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using Data.Interfaces.DataTransferObjects;

namespace terminalSalesforce.Actions
{
    public class Salesforce_Get_Data_v1 : BaseTerminalAction
    {
        ISalesforceIntegration _salesforce = new SalesforceIntegration();

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            //if empty crate storage, proceed with initial config
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            //if no salesforce object is selected, proceed with initial config
            string selectedSalesForceObject = ((DropDownList) GetControl(curActionDO, "WhatKindOfData", ControlTypes.DropDownList)).selectedKey;
            if (string.IsNullOrEmpty(selectedSalesForceObject))
            {
                return ConfigurationRequestType.Initial;
            }

            //proceed with follow up conifig if the above use cases are failed.
            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //create hard coded salesforce object names as design time fields.
            var availableSalesforceObjects = Crate.CreateDesignTimeFieldsCrate("AvailableSalesforceObjects",
                new FieldDTO[] { new FieldDTO("Account"), new FieldDTO("Contact"), new FieldDTO("Lead") });

            //Note: This design time fields are used to populate the Fileter Pane controls. It has to be labelled as Queryable Criteria
            var emptyFieldsSource = Crate.CreateDesignTimeFieldsCrate("Queryable Criteria", new FieldDTO[] {});

            //create required controls
            var configurationControlsCrate = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
            {
                updater.CrateStorage = AssembleCrateStorage(availableSalesforceObjects, emptyFieldsSource, configurationControlsCrate);
            }
            
            return await Task.FromResult(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO,
                                                                              AuthorizationTokenDO authTokenDO)
        {


            return await Task.FromResult(curActionDO);
        }

        private Crate CreateControlsCrate()
        {
            //DDLB for What Salesforce Object to be considered
            var whatKindOfData = new DropDownList
            {
                Name = "WhatKindOfData",
                Required = true,
                Label = "What kind of Salesforce object do you want to get data?",
                Source = new FieldSourceDTO
                {
                    Label = "AvailableSalesforceObjects",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
            };

            //Filter Pane control for the user to filter objects based on their fields values
            var fieldFilterPane = new FilterPane()
            {
                Label = "Get Salesforce Object Data by filtering Fields values",
                Name = "SelectedFilter",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Queryable Criteria",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            return PackControlsCrate(whatKindOfData, fieldFilterPane);
        }
    }
}