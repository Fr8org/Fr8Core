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
        StandardAccountingTransactionCM GetAccountingTransactionData(Intuit.Ipp.Data.JournalEntry journalEntry);
        Intuit.Ipp.Data.JournalEntry GetJournalEntryFromCM(StandardAccountingTransactionCM crate);
        void Create(StandardAccountingTransactionCM curStandardAccountingTransactionCm, AuthorizationTokenDO authTokenDo);
    }
}