using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Diagnostics;
using StructureMap;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    public class JournalEntry : IJournalEntry
    {
        private QuickBooksIntegration _quickBooksIntegration;
        private DataService _dataService;
        public JournalEntry()
        {
            //_quickBooksIntegration = ObjectFactory.GetInstance<IQuickBooksIntegration>();
            _quickBooksIntegration = new QuickBooksIntegration();
        }
        /// <summary>
        /// Converts JournalEntry to StandardAccountingTransactionCM object
        /// </summary>
        /// <param name="JournalEntry"></param>
        /// <returns>JournalEntry</returns>
        public StandardAccountingTransactionCM GetAccountingTransactionData(Intuit.Ipp.Data.JournalEntry journalEntry)
        {
            var curFinLineDTOList = new List<FinancialLineDTO>();
            foreach (var curLine in journalEntry.Line)
            {
                var curFinLineToAdd = new FinancialLineDTO();
                
                //The way to extract Journal Entry Line Detail type object from the Line
                JournalEntryLineDetail curJournalEntryLineDetail = (JournalEntryLineDetail) curLine.AnyIntuitObject;
                //Add Debit or Credit type
                curFinLineToAdd.DebitOrCredit = curJournalEntryLineDetail.PostingType.ToString();
                //Add Amount
                curFinLineToAdd.Amount = curLine.Amount.ToString();
                //Add Account Name
                curFinLineToAdd.AccountName = curJournalEntryLineDetail.AccountRef.name;
                //Add Account Id
                curFinLineToAdd.AccountId = curJournalEntryLineDetail.AccountRef.Value;
                //Add Description
                curFinLineToAdd.Description = curLine.Description;
                //Add the prepared line to the list
                curFinLineDTOList.Add(curFinLineToAdd);
            }
            var curTransactionCrate = new StandardAccountingTransactionCM();
            
            var curAccountTransactionDTO = new StandardAccountingTransactionDTO()
            {
                Name = journalEntry.DocNumber,
                TransactionDate = journalEntry.TxnDate,
                FinancialLines = curFinLineDTOList,
                Memo = journalEntry.PrivateNote
            };
            curTransactionCrate.AccountingTransactionDTO = curAccountTransactionDTO;
            return curTransactionCrate;
        }
        /// <summary>
        /// Converts StandardAccountingTransactionCM to JournalEntry object
        /// </summary>
        /// <param name="StandardAccountingTransactionCM"></param>
        /// <returns>JournalEntry</returns>
        public Intuit.Ipp.Data.JournalEntry GetJournalEntryFromCM(StandardAccountingTransactionCM crate)
        {
            var curJournalEntry = new Intuit.Ipp.Data.JournalEntry();
            var curAccountTransactionDTO = crate.AccountingTransactionDTO;
            //Pack Standard Accounting Transaction DTO with data
            //Add DocNumber
            curJournalEntry.DocNumber = curAccountTransactionDTO.Name;
            //Add Date
            curJournalEntry.TxnDate = curAccountTransactionDTO.TransactionDate;
            //Add Memo
            curJournalEntry.PrivateNote = curAccountTransactionDTO.Memo;
            var curNumOfFinLineDTOs = curAccountTransactionDTO.FinancialLines.Count();
            var curLineArray = new Line[curNumOfFinLineDTOs];
            for (int i = 0; i < curNumOfFinLineDTOs; i++)
            {
                var curFinLineDTO = curAccountTransactionDTO.FinancialLines[i];
                var curLineToAdd = new Line();
                //Add Description
                curLineToAdd.Description = curFinLineDTO.Description;
                //Add Account Id
                curLineToAdd.Id = curFinLineDTO.AccountId;
                //Add Debit or Credit type
                var curJournalEntryLineDetail = new JournalEntryLineDetail();
                curJournalEntryLineDetail.PostingType = ParseEnum<PostingTypeEnum>(curFinLineDTO.DebitOrCredit);
                //Add AccountRef and add name
                var curAccountRef = new ReferenceType();
                curAccountRef.name = curFinLineDTO.AccountName;
                curJournalEntryLineDetail.AccountRef = curAccountRef;
                curLineToAdd.AnyIntuitObject = curJournalEntryLineDetail;
                //Add Amount
                curLineToAdd.Amount = decimal.Parse(curFinLineDTO.Amount);
                //Pack the line to the array
                curLineArray[i] = curLineToAdd;
            }

            curJournalEntry.Line = curLineArray;
            return curJournalEntry;
        }
        public void Create(StandardAccountingTransactionCM crate, AuthorizationTokenDO authTokenDO)
        {
            var curJournalEntry = GetJournalEntryFromCM(crate);
            var curDataService = _quickBooksIntegration.GetDataService(authTokenDO);
            try
            {
                curDataService.Add(curJournalEntry);
            }
            catch(Exception curException)
            {
                throw curException;
            }
        }
        private static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}