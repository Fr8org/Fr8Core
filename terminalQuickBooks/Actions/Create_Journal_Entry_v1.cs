using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using JournalEntry = terminalQuickBooks.Services.JournalEntry;

namespace terminalQuickBooks.Actions
{
    public class Create_Journal_Entry_v1 : BaseTerminalAction
    {
        private JournalEntry _journalEntry;
        public Create_Journal_Entry_v1()
        {
            _journalEntry = new JournalEntry();
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
            var processPayload = await GetProcessPayload(curActionDO, containerId);
            //Obtain the crate of type StandardAccountingTransactionCM
            var curStandardAccountingTransactionCM = Crate.FromDto(processPayload.CrateStorage).CratesOfType<StandardAccountingTransactionCM>().Single().Content;
            //Validate fields of the StandardAccountingTransactionCM crate
            ValidateStandardAccountingTransactionCM(curStandardAccountingTransactionCM);
            //Get the number of Accounting Transactions for iteration
            var curNumberOfTransactions = curStandardAccountingTransactionCM.AccountingTransactionDTOList.Count;
            //Iterate through all transactions that are inside of the StandardAccountingTransactionCM crate
            for (int i = 0; i < curNumberOfTransactions; i++)
            {
                //Take StandardAccountingTransactionDTO from curStandardAccountingTransactionCM crate
                var curStandardAccountingTransactionDTO = curStandardAccountingTransactionCM.AccountingTransactionDTOList[i];
                //Check that all required fields exists in the StandardAccountingTransactionDTO object
                ValidateAccountingTransation(curStandardAccountingTransactionDTO);
                //Use service to create Journal Entry Object
                _journalEntry.Create(curStandardAccountingTransactionDTO, authTokenDO);
            }
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
                if (upstream.Count != 0)
                {
                    textBlock = GenerateTextBlock("Create a Journal Entry",
                        "This Action doesn't require any configuration.",
                        "well well-lg");
                    using (var updater = Crate.UpdateStorage(curActionDO))
                    {
                        updater.CrateStorage.Add(upstream[0]);
                    }
                }
                else
                {
                    textBlock = GenerateTextBlock("Create a Journal Entry",
                        "When this Action runs, it will be expecting to find a Crate of Standard Accounting Transactions. Right now, it doesn't detect any Upstream Actions that produce that kind of Crate. Please add an action upstream (to the left) of this action that does so.",
                        "alert alert-warning");
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

        private void ValidateAccountingTransation(StandardAccountingTransactionDTO curAccountingTransactionDtoTransactionDTO)
        {
            if (curAccountingTransactionDtoTransactionDTO == null)
            {
                throw new ArgumentNullException("No StandardAccountingTransationDTO provided");
            }
            if (curAccountingTransactionDtoTransactionDTO.FinancialLines == null 
                || curAccountingTransactionDtoTransactionDTO.FinancialLines.Count == 0
                || curAccountingTransactionDtoTransactionDTO.TransactionDate == null)
            {
                throw new Exception("No Financial Lines or Transaction Date Provided");
            }
            foreach (var curFinLineDTO in curAccountingTransactionDtoTransactionDTO.FinancialLines)
            {
                ValidateFinancialLineDTO(curFinLineDTO);
            }
        }

        private void ValidateFinancialLineDTO(FinancialLineDTO finLineDTO)
        {
            if (finLineDTO.AccountId == null || finLineDTO.AccountName == null)
            {
                throw new Exception("Some Account Data is Missing");
            }
            if (finLineDTO.Amount == null)
            {
                throw new Exception("Amount is missing");
            }
            if (finLineDTO.DebitOrCredit == null)
            {
                throw new Exception("Debit/Credit information is missing");
            }
        }

        private void ValidateStandardAccountingTransactionCM(StandardAccountingTransactionCM crate)
        {
            if (crate.AccountingTransactionDTOList == null)
            {
                throw new NullReferenceException("AccountingTransactionDTOList is null");
            }
            if (crate.AccountingTransactionDTOList.Count == 0)
            {
                throw new Exception("No Transactions in the AccountingTransactionDTOList");
            }
        }
    }
}