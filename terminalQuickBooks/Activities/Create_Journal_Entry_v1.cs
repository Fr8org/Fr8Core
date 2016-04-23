using System;
using System.Threading.Tasks;
using Data.Control;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using JournalEntry = terminalQuickBooks.Services.JournalEntry;

namespace terminalQuickBooks.Activities
{
    public class Create_Journal_Entry_v1 : BaseTerminalActivity
    {
        private JournalEntry _journalEntry;
        public Create_Journal_Entry_v1()
        {
            _journalEntry = new JournalEntry();
        }

        public async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, dto => ConfigurationRequestType.Initial, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActivityDO.Id != Guid.Empty)
            {
                //get StandardAccountingTransactionCM
                var upstream = await GetCratesByDirection<StandardAccountingTransactionCM>(curActivityDO, CrateDirection.Upstream);
                //In order to Create Journal Entry an upstream action needs to provide a StandardAccountingTransactionCM.
                TextBlock textBlock;
                if (upstream.Count != 0)
                {
                    textBlock = GenerateTextBlock("Create a Journal Entry",
                        "This Action doesn't require any configuration.",
                        "well well-lg");
                    using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                    {
                        crateStorage.Add(upstream[0]);
                    }
                }
                else
                {
                    textBlock = GenerateTextBlock("Create a Journal Entry",
                        "When this Action runs, it will be expecting to find a Crate of Standard Accounting Transactions. Right now, it doesn't detect any Upstream Actions that produce that kind of Crate. Please add an activity upstream (to the left) of this action that does so.",
                        "alert alert-warning");
                }
                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.Clear();
                    crateStorage.Add(PackControlsCrate(textBlock));
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
            return curActivityDO;
        }
        //It is assumed that Action is the child of the Loop action.
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            
            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }
            
            //Obtain the crate of type StandardAccountingTransactionCM that holds the required information
            var curStandardAccountingTransactionCM = CrateManager.GetByManifest<StandardAccountingTransactionCM>(payloadCrates);
            //Obtain the crate of type OperationalStateCM to extract the correct StandardAccountingTransactionDTO
            var curOperationalStateCM = CrateManager.GetOperationalState(payloadCrates);
            //Get the LoopId that is equal to the Action.Id for to obtain the correct StandardAccountingTransactionDTO
            //Validate fields of the StandardAccountingTransactionCM crate
            StandardAccountingTransactionCM.Validate(curStandardAccountingTransactionCM);
            //Get the list of the StandardAccountingTransactionDTO
            var curTransactionList = curStandardAccountingTransactionCM.AccountingTransactions;
            //Get the current index of Accounting Transactions
            var currentIndexOfTransactions = GetLoopIndex(curOperationalStateCM);
            //Take StandardAccountingTransactionDTO from curTransactionList using core function GetCurrentElement
            var curStandardAccountingTransactionDTO = (StandardAccountingTransactionDTO)GetCurrentElement(curTransactionList, currentIndexOfTransactions);
            //Check that all required fields exists in the StandardAccountingTransactionDTO object
            StandardAccountingTransactionCM.ValidateAccountingTransation(curStandardAccountingTransactionDTO);
            //Use service to create Journal Entry Object
            _journalEntry.Create(curStandardAccountingTransactionDTO, authTokenDO);
            return payloadCrates;
        }     
    }
}