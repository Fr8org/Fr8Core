using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;

namespace terminalSalesforce.Actions
{
    public class Create_Account_v1 : BaseTerminalAction
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


            var accountName = ExtractControlFieldValue(curActionDO, "accountName");
            if (string.IsNullOrEmpty(accountName))
            {
                throw new ApplicationException("No account name found in action.");
            }


            bool result = _salesforce.CreateAccount(curActionDO, authTokenDO);


            return processPayload;
        }

        private ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var accountName = new TextBox()
            {
                Label = "Account Name",
                Name = "accountName",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }

            };
            var accountNumber = new TextBox()
            {
                Label = "Account Number",
                Name = "accountNumber",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var phone = new TextBox()
            {
                Label = "Phone",
                Name = "phone",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var controls = PackControlsCrate(accountName, accountNumber, phone);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(controls);
            }

            return await Task.FromResult(curActionDO);
        }
    }
}