using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Intuit.Ipp.Data;
using StructureMap;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    /// <summary>
    /// This class it the only one to communicate with action
    /// It performs authorization using DataService instanticating QuickBooksIntegration
    /// </summary>
    public class JournalEntry : IJournalEntry
    {
        private readonly IServiceWorker _serviceWorker;

        public JournalEntry(IServiceWorker serviceWorker)
        {
            _serviceWorker = serviceWorker;
        }

        /// <summary>
        /// Converts JournalEntry to StandardAccountingTransactionDTO object
        /// </summary>
        /// <param name="JournalEntry"></param>
        /// <returns>JournalEntry</returns>
        public StandardAccountingTransactionDTO GetAccountingTransactionData(Intuit.Ipp.Data.JournalEntry journalEntry)
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
            
            var curAccountTransactionDTO = new StandardAccountingTransactionDTO()
            {
                Name = journalEntry.DocNumber,
                TransactionDate = journalEntry.TxnDate,
                FinancialLines = curFinLineDTOList,
                Memo = journalEntry.PrivateNote
            };
            return curAccountTransactionDTO;
        }
        /// <summary>
        /// Converts StandardAccountingTransactionDTO to JournalEntry object
        /// </summary>
        /// <param name="StandardAccountingTransactionCM"></param>
        /// <returns>JournalEntry</returns>
        public Intuit.Ipp.Data.JournalEntry CreateQbJournalEntry(StandardAccountingTransactionDTO curAccountTransactionDTO)
        {
            //Pack Standard Accounting Transaction DTO with data
            var curJournalEntry = new Intuit.Ipp.Data.JournalEntry
            {
                DocNumber = curAccountTransactionDTO.Name,
                TxnDate = curAccountTransactionDTO.TransactionDate,
                PrivateNote = curAccountTransactionDTO.Memo
            };

            var curFinancialLines = curAccountTransactionDTO.FinancialLines;
            var curTransactionList = new List<Line>();
            foreach (var curTransaction in curFinancialLines)
            {
                var curLineToAdd = new Line
                {
                    Description = curTransaction.Description,
                    Id = curTransaction.AccountId
                };
               
                //Add Debit or Credit type
                var curJournalEntryLineDetail = new JournalEntryLineDetail
                {
                    PostingType = ParseEnum<PostingTypeEnum>(curTransaction.DebitOrCredit)
                };
                //Add AccountRef and add name
                var curAccountRef = new ReferenceType
                {
                    name = curTransaction.AccountName
                };
                curJournalEntryLineDetail.AccountRef = curAccountRef;
                curLineToAdd.AnyIntuitObject = curJournalEntryLineDetail;
                //Add Amount
                curLineToAdd.Amount = decimal.Parse(curTransaction.Amount);
                //Pack the line to the array
                curTransactionList.Add(curLineToAdd);
            }
            Line[] curLineArray = curTransactionList.ToArray();
            curJournalEntry.Line = curLineArray;
            return curJournalEntry;
        }

        /// <summary>
        /// Creates Journal Entry in the developers account in Sandbox in Intuit https://sandbox.qbo.intuit.com/app/journal
        /// </summary>
        public void Create(
            StandardAccountingTransactionDTO curAccountingTransactionDto, 
            AuthorizationToken authToken,
            IHubCommunicator hubCommunicator)
        {
            var curJournalEntry = CreateQbJournalEntry(curAccountingTransactionDto);
            var curDataService = _serviceWorker.GetDataService(authToken, hubCommunicator);
            try
            {
                curDataService.Add(curJournalEntry);
            }
            catch(Exception curException)
            {
                throw curException;
            }
        }
       
         /// <summary>
         /// This method is used to convert DebitOrCredit string value to PostingTypeEnum Enumerator
         /// </summary>
         /// <typeparam name="T"></typeparam>
         /// <param name="value"></param>
         /// <returns></returns>
        private static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}