using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Migrations;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Hub.StructureMap;
using Intuit.Ipp.Data;
using NUnit.Framework;
using StructureMap;
using terminalQuickBooks.Services;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using JournalEntry = terminalQuickBooks.Services.JournalEntry;
using Task = System.Threading.Tasks.Task;

namespace terminalQuickBooks.Tests.Services
{
    public class JournalEntryTests : BaseTest
    {
        private JournalEntry _journalEntry;
        private Connectivity _connectivity;
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            _journalEntry = new JournalEntry();
            _connectivity = new Connectivity();
        }
        [Test]
        public void JournalEntryService_ConvertsCrate_To_JouralEntry()
        {
            //Assign
            var _journalEntry = new JournalEntry();
            var curCrate = Fixtures.Fixtures.GetAccountingTransactionCM();
            var curTransactionDTO = curCrate.AccountingTransactions[0];
            //Act
            var journalEntry = _journalEntry.CreateQbJournalEntry(curTransactionDTO);
            //Assert First Line
            Assert.AreEqual("100",journalEntry.Line[0].Amount.ToString());
            Assert.AreEqual("1",journalEntry.Line[0].Id.ToString());
            var firstLineDetails = (JournalEntryLineDetail) journalEntry.Line[0].AnyIntuitObject;
            Assert.AreEqual("Account-A", firstLineDetails.AccountRef.name);
            Assert.AreEqual("Debit", firstLineDetails.PostingType.ToString());
            Assert.AreEqual("Move money to Accout-B", journalEntry.Line[0].Description);
            //Assert Second Line
            Assert.AreEqual("100", journalEntry.Line[1].Amount.ToString());
            Assert.AreEqual("2", journalEntry.Line[1].Id.ToString());
            var secondLineDetails = (JournalEntryLineDetail)journalEntry.Line[1].AnyIntuitObject;
            Assert.AreEqual("Account-B", secondLineDetails.AccountRef.name);
            Assert.AreEqual("Credit", secondLineDetails.PostingType.ToString());
            Assert.AreEqual("Move money from Accout-A", journalEntry.Line[1].Description);
            //Assert Journal Entry Data
            Assert.AreEqual("Code1", journalEntry.DocNumber);
            Assert.AreEqual(DateTime.Parse("2015-12-15"), journalEntry.TxnDate);
            Assert.AreEqual("That is the test crate", journalEntry.PrivateNote);
        }
        [Test]
        public void JournalEntryService_ConvertJournalEntry_To_Crate()
        {
            //Assign
            var _journalEntry=new JournalEntry();
            var curJournalEntry = Fixtures.Fixtures.CreateJournalEntry();
            //Act
            var curTransactionDTO = _journalEntry.GetAccountingTransactionData(curJournalEntry);
            //Assert General Data
            Assert.AreEqual("DocNu1", curTransactionDTO.Name);
            Assert.AreEqual(DateTime.UtcNow.Date, curTransactionDTO.TransactionDate);
            Assert.AreEqual("This is the test Journal Entry", curTransactionDTO.Memo);
            //Assert First Line
            var curFirstLine = curTransactionDTO.FinancialLines[0];
            Assert.AreEqual("36", curFirstLine.AccountId);
            Assert.AreEqual("Accumulated Depreciation", curFirstLine.AccountName);
            Assert.AreEqual("100", curFirstLine.Amount.ToString());
            Assert.AreEqual(PostingTypeEnum.Debit.ToString(), curFirstLine.DebitOrCredit);
            Assert.AreEqual("That is the first line description", curFirstLine.Description);
            //Assert Second Line
            var curSecondLine = curTransactionDTO.FinancialLines[1];
            Assert.AreEqual("36", curSecondLine.AccountId);
            Assert.AreEqual("Accumulated Depreciation", curSecondLine.AccountName);
            Assert.AreEqual("100", curSecondLine.Amount.ToString());
            Assert.AreEqual(PostingTypeEnum.Credit.ToString(), curSecondLine.DebitOrCredit);
            Assert.AreEqual("That is the second line description", curSecondLine.Description);
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
            var curJournalEntry = _journalEntry.CreateQbJournalEntry(curAccountingTransactionDto);
            var curDataService = _connectivity.GetDataService(authTokenDO);
            Intuit.Ipp.Data.JournalEntry resultJournalEntry;
            try
            {
                resultJournalEntry = curDataService.FindAll(curJournalEntry).ToList().First();
                return _journalEntry.GetAccountingTransactionData(resultJournalEntry);
            }
            catch (Exception curException)
            {
                throw curException;
            }
        }
    }
}