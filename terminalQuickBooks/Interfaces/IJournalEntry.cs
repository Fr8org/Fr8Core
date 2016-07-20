using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace terminalQuickBooks.Interfaces
{
    public interface IJournalEntry
    {
        StandardAccountingTransactionDTO GetAccountingTransactionData(Intuit.Ipp.Data.JournalEntry journalEntry);
        Intuit.Ipp.Data.JournalEntry CreateQbJournalEntry(StandardAccountingTransactionDTO crate);
        void Create(
            StandardAccountingTransactionDTO curStandardAccountingTransactionCM,
            AuthorizationToken authorizationToken,
            IHubCommunicator hubCommunicator);
    }
}