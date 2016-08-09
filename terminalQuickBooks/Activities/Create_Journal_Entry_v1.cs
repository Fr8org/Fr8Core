using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Actions
{
    public class Create_Journal_Entry_v1 : BaseQuickbooksTerminalActivity<Create_Journal_Entry_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM { }

        private readonly IJournalEntry _journalEntry;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("8d1d8407-488f-4494-a724-746c1ae4e901"),
            Version = "1",
            Name = "Create_Journal_Entry",
            Label = "Create Journal Entry",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        public Create_Journal_Entry_v1(ICrateManager crateManager, IJournalEntry journalEntry)
            : base(crateManager)
        {
            _journalEntry = journalEntry;
        }

        public override async Task Initialize()
        {
            if (ActivityId == Guid.Empty)
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
          
        }

        public override Task FollowUp()
        {
            // No extra configuration required
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            //Obtain the crate of type StandardAccountingTransactionCM that holds the required information
            var curStandardAccountingTransactionCM = Payload.CratesOfType<StandardAccountingTransactionCM>().Single().Content;
            //Obtain the crate of type OperationalStateCM to extract the correct StandardAccountingTransactionDTO
            var curOperationalStateCM = Payload.CratesOfType<OperationalStateCM>().Single().Content;
            //Get the LoopId that is equal to the Action.Id for to obtain the correct StandardAccountingTransactionDTO
            //Validate fields of the StandardAccountingTransactionCM crate
            StandardAccountingTransactionCM.Validate(curStandardAccountingTransactionCM);
            //Get the list of the StandardAccountingTransactionDTO
            var curTransactionList = curStandardAccountingTransactionCM.AccountingTransactions;
            //Take StandardAccountingTransactionDTO from curTransactionList using core function GetCurrentElement
            var curStandardAccountingTransactionDTO = curTransactionList[LoopIndex];
            //Check that all required fields exists in the StandardAccountingTransactionDTO object
            StandardAccountingTransactionCM.ValidateAccountingTransation(curStandardAccountingTransactionDTO);
            //Use service to create Journal Entry Object
            _journalEntry.Create(curStandardAccountingTransactionDTO, AuthorizationToken, HubCommunicator);
        }
    }
}