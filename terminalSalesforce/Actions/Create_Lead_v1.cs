using Data.Interfaces.DataTransferObjects;
using StructureMap;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using Salesforce.Common;
using Hub.Managers;
using terminalSalesforce.Services;
using TerminalBase.Infrastructure;
using System.Collections.Generic;
using Data.Entities;
using TerminalBase.BaseClasses;
using System;
using Data.Control;

namespace terminalSalesforce.Actions
{
    public class Create_Lead_v1 : BaseTerminalAction
    {
        ISalesforceIntegration _salesforce = new SalesforceIntegration();

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, x => ConfigurationEvaluator(x), authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            PayloadDTO processPayload = null;

            processPayload = await GetProcessPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
                {
                    throw new ApplicationException("No AuthToken provided.");
                }


            var lastName = ExtractControlFieldValue(curActionDO, "lastName");
                if (string.IsNullOrEmpty(lastName))
                {
                    throw new ApplicationException("No last name found in action.");
                }

                var company = ExtractControlFieldValue(curActionDO, "companyName");
                if (string.IsNullOrEmpty(company))
                {
                    throw new ApplicationException("No company name found in action.");
                }

                bool result = _salesforce.CreateLead(curActionDO, authTokenDO);
           
          
            return processPayload;
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var firstNameCrate = new TextBox()
            {
                Label = "First Name",
                Name = "firstName",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }

            };
            var lastNAme = new TextBox()
            {
                Label = "Last Name",
                Name = "lastName",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };
            var company = new TextBox()
            {
                Label = "Company ",
                Name = "companyName",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(firstNameCrate, lastNAme, company));
            }

            return await Task.FromResult(curActionDO);
        }
    }
}