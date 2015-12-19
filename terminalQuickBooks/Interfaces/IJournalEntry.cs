using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Intuit.Ipp.DataService;

namespace terminalQuickBooks.Interfaces
{
    public interface IJournalEntry
    {
        StandardAccountingTransactionDTO GetAccountingTransactionData(Intuit.Ipp.Data.JournalEntry journalEntry);
        Intuit.Ipp.Data.JournalEntry GetJournalEntryFromAccountingTransactionDTO(StandardAccountingTransactionDTO crate);
        void Create(StandardAccountingTransactionDTO curStandardAccountingTransactionCm, AuthorizationTokenDO authTokenDo);
    }
}