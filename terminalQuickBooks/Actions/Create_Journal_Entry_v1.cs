using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Data.Crates;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Diagnostics;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;
using JournalEntry = terminalQuickBooks.Services.JournalEntry;

namespace terminalQuickBooks.Actions
{
    public class Create_Journal_Entry_v1 : BaseTerminalAction
    {
        private  JournalEntry _journalEntry;
        public Create_Journal_Entry_v1()
        {
            _journalEntry  = new JournalEntry();
        }
        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }
            return await ProcessConfigurationRequest(curActionDO, dto => ConfigurationRequestType.Initial, authTokenDO);
        }
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
                throw new ApplicationException("No AuthToken provided.");
            //var processPayload = await GetProcessPayload(curActionDO, containerId);
            var processPayload = new PayloadDTO( containerId);
            var curStandardAccountingTransactionCM = Crate.GetStorage(curActionDO).CratesOfType<StandardAccountingTransactionCM>().Single().Content;
            CheckAccountingTransationCM(curStandardAccountingTransactionCM);
            _journalEntry.Create(curStandardAccountingTransactionCM, authTokenDO);
            return processPayload;  
        }
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                //get StandardAccountingTransactionCM
                var upstream = await GetCratesByDirection<StandardAccountingTransactionCM>(curActionDO, CrateDirection.Upstream);
                //In order to Create Journal Entry an upstream action needs to provide a StandardAccountingTransactionCM.
                TextBlock textBlock;
                if (upstream.Count!=0)
                {
                    textBlock = new TextBlock
                    {
                        Label = "Create a Journal Entry",
                        Value = "This Action doesn't require any configuration.",
                        CssClass = "well well-lg"
                    };
                    using (var updater = Crate.UpdateStorage(curActionDO))
                    {
                        updater.CrateStorage.Add(upstream[0]);
                    }
                }
                else
                {
                    textBlock = new TextBlock
                    {
                        Label = "Create Journal Entry",
                        Value = "In order to Create a Journal Entry, an upstream action needs to provide a StandardAccountingTransactionCM.",
                        CssClass = "alert alert-warning"
                    };
                }
                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.Clear();
                    updater.CrateStorage.Add(PackControlsCrate(textBlock));
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
            return curActionDO;
        }

        private void CheckAccountingTransationCM(StandardAccountingTransactionCM accountingTransactionCrate)
        {
            if (accountingTransactionCrate == null)
            {
                throw new ArgumentNullException("No StandardAccountingTransationCM provided");
            }
            if (accountingTransactionCrate.AccountingTransactionDTO == null)
            {
                throw new NullReferenceException("No StandardAccountingTransationDTO inside StandardAccountingTransationCM");
            }
            var curAccTransactionDTO = accountingTransactionCrate.AccountingTransactionDTO;
            
            if (curAccTransactionDTO.FinancialLines==null || curAccTransactionDTO.TransactionDate==null)
            {
                throw new Exception("No Financial Lines or Transaction Date Provided");
            }
            foreach (var curFinLineDTO in accountingTransactionCrate.AccountingTransactionDTO.FinancialLines)
            {
                CheckFinancialLineDTO(curFinLineDTO);
            }
        }

        private void CheckFinancialLineDTO(FinancialLineDTO finLineDTO)
        {
            if (finLineDTO.AccountId == null || finLineDTO.AccountName==null)
            {
                throw new Exception("Some Account Data is Missing");
            }
            if (finLineDTO.Amount==null)
            {
                throw new Exception("Amount is missing");
            }
            if (finLineDTO.DebitOrCredit == null)
            {
                throw new Exception("Debit/Credit information is missing");
            }
        }
    }
}