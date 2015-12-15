using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Diagnostics;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;
using JournalEntry = Intuit.Ipp.Data.JournalEntry;

namespace terminalQuickBooks.Actions
{
    public class Create_Journal_Entry_v1 : BaseTerminalAction
    {
        private IQuickBooksIntegration _quickBooksIntegration = new QuickBooksIntegration();

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
                throw new ApplicationException("No AuthToken provided.");

            return curActionDO;
        }
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId,
    AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
                throw new ApplicationException("No AuthToken provided.");
            var processPayload = await GetProcessPayload(curActionDO, containerId);
            return processPayload;  
        }
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (true)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}