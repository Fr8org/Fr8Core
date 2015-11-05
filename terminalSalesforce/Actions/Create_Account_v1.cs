using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;

namespace terminalSalesforce.Actions
{
    public class Create_Account_v1 : BasePluginAction
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

        public object Activate(ActionDO curActionDO)
        {
            //not implemented currently
            return null;
        }

        public object Deactivate(ActionDO curActionDO)
        {
            //not implemented currentlys
            return "Deactivated";
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, int containerId, AuthorizationTokenDO authTokenDO)
        {
            PayloadDTO processPayload = null;

            processPayload = await GetProcessPayload(containerId);

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

        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            var accountName = new TextBoxControlDefinitionDTO()
            {
                Label = "Account Name",
                Name = "accountName",
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }

            };
            var accountNumber = new TextBoxControlDefinitionDTO()
            {
                Label = "Account Number",
                Name = "accountNumber",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var phone = new TextBoxControlDefinitionDTO()
            {
                Label = "Phone",
                Name = "phone",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            var controls = PackControlsCrate(accountName, accountNumber, phone);
            var curCrateDTOList = new List<CrateDTO>();
            curCrateDTOList.Add(controls);
            curActionDO.UpdateCrateStorageDTO(curCrateDTOList);

            return await Task.FromResult(curActionDO);
        }
    }
}