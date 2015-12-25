using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
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
            CheckAuthentication(authTokenDO);
            return await ProcessConfigurationRequest(curActionDO, dto => ConfigurationRequestType.Initial, authTokenDO);
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
        //It is assumed that Action is the child of the Loop action.
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);
            var processPayload = await GetProcessPayload(curActionDO, containerId);
            //Obtain the crate of type StandardAccountingTransactionCM that holds the required information
            var curStandardAccountingTransactionCM = Crate.GetByManifest<StandardAccountingTransactionCM>(processPayload);
            //Obtain the crate of type OperationalStateCM to extract the correct StandardAccountingTransactionDTO
            var curOperationalStateCM = Crate.GetOperationalState(processPayload);
            //Get the LoopId that is equal to the Action.Id for to obtain the correct StandardAccountingTransactionDTO
            var curLoopId = curActionDO.GetLoopId();
            //Validate fields of the StandardAccountingTransactionCM crate
            StandardAccountingTransactionCM.Validate(curStandardAccountingTransactionCM);
            //Get the list of the StandardAccountingTransactionDTO
            var curTransactionList = curStandardAccountingTransactionCM.AccountingTransactions;
            //Get the current index of Accounting Transactions
            var currentIndexOfTransactions = GetLoopIndex(curOperationalStateCM, curLoopId);
            //Take StandardAccountingTransactionDTO from curTransactionList using core function GetCurrentElement
            var curStandardAccountingTransactionDTO = (StandardAccountingTransactionDTO)GetCurrentElement(curTransactionList, currentIndexOfTransactions);
            //Check that all required fields exists in the StandardAccountingTransactionDTO object
            StandardAccountingTransactionCM.ValidateAccountingTransation(curStandardAccountingTransactionDTO);
            //Use service to create Journal Entry Object
            _journalEntry.Create(curStandardAccountingTransactionDTO, authTokenDO);
            return processPayload;
        }     
    }
}