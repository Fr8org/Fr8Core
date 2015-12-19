using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    /// <summary>
    /// This class it the only one to communicate with action
    /// It performs authorization using DataService instanticating QuickBooksIntegration
    /// </summary>
    public class JournalEntry : IJournalEntry
    {
        private QuickBooksIntegration _quickBooksIntegration;
        private DataService _dataService;
        public JournalEntry()
        {
            _quickBooksIntegration = new QuickBooksIntegration();
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
        public Intuit.Ipp.Data.JournalEntry GetJournalEntryFromAccountingTransactionDTO(StandardAccountingTransactionDTO curAccountTransactionDTO)
        {
            var curJournalEntry = new Intuit.Ipp.Data.JournalEntry();
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
        /// <summary>
        /// Creates Journal Entry in the developers account in Sandbox in Intuit https://sandbox.qbo.intuit.com/app/journal
        /// </summary>
        /// <param name="StandardAccountingTransactionCM"></param>
        /// <param name="authTokenDO"></param>
        public void Create(StandardAccountingTransactionDTO curAccountingTransactionDto, AuthorizationTokenDO authTokenDO)
        {
            var curJournalEntry = GetJournalEntryFromAccountingTransactionDTO(curAccountingTransactionDto);
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
        /// <summary>
        /// Method is created for testing purposes
        /// It takes StandardAccountingTransactionDTO as an input, converts it into journal entry, looks for similar journal entries in the Sandbox,
        /// takes first occurance from the list, and returns converted back crate object
        /// </summary>
        /// <param name="StandardAccountingTransactionDTO"></param>
        /// <param name="authTokenDO"></param>
        /// <returns></returns>
        public StandardAccountingTransactionDTO Find(StandardAccountingTransactionDTO curAccountingTransactionDto, AuthorizationTokenDO authTokenDO)
        {
            var curJournalEntry = GetJournalEntryFromAccountingTransactionDTO(curAccountingTransactionDto);
            var curDataService = _quickBooksIntegration.GetDataService(authTokenDO);
            Intuit.Ipp.Data.JournalEntry resultJournalEntry;
            try
            {
                 resultJournalEntry = curDataService.FindAll(curJournalEntry).ToList().First();
                 return GetAccountingTransactionData(resultJournalEntry);
            }
            catch (Exception curException)
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