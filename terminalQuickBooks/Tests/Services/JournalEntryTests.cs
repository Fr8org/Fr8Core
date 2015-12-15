using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Intuit.Ipp.Data;
using NUnit.Framework;
using StructureMap;
using terminalQuickBooks.Interfaces;
using terminalQuickBooks.Services;
using UtilitiesTesting;
using JournalEntry = terminalQuickBooks.Services.JournalEntry;

namespace terminalQuickBooks.Tests.Services
{
    public class JournalEntryTests : BaseTest
    {
        //private IJournalEntry _journalEntry;

        //public override void SetUp()
        //{
        //    base.SetUp();

        //    _journalEntry = ObjectFactory.GetInstance<IJournalEntry>();
        //}

        [Test]
        public void JournalEntryService_ConvertsCrate_To_JouralEntry()
        {
            //Assign
            var _journalEntry = new JournalEntry();
            var curCrate = Fixtures.Fixtures.GetAccountingTransactionCM();
            //Act
            var journalEntry = _journalEntry.GetJournalEntryFromCM(curCrate);
            //Assert First Line
            Assert.AreEqual("100",journalEntry.Line[0].Amount.ToString());
            Assert.AreEqual("1",journalEntry.Line[0].Id.ToString());
            var firstLineDetails = (JournalEntryLineDetail) journalEntry.Line[0].AnyIntuitObject;
            Assert.AreEqual("Account-A", firstLineDetails.AccountRef.name);
            Assert.AreEqual("Debit", firstLineDetails.PostingType.ToString());
            //Assert Second Line
            Assert.AreEqual("100", journalEntry.Line[1].Amount.ToString());
            Assert.AreEqual("2", journalEntry.Line[1].Id.ToString());
            var secondLineDetails = (JournalEntryLineDetail)journalEntry.Line[1].AnyIntuitObject;
            Assert.AreEqual("Account-B", secondLineDetails.AccountRef.name);
            Assert.AreEqual("Credit", secondLineDetails.PostingType.ToString());
            //Assert Journal Entry Data
            Assert.AreEqual("Code1", journalEntry.DocNumber);
            Assert.AreEqual(DateTime.Parse("2015-12-15"), journalEntry.TxnDate);
        }

        [Test]
        public void JournalEntryService_ConvertJournalEntry_To_Crate()
        {
            //Assign
            var _journalEntry=new JournalEntry();
            var curJournalEntry = Fixtures.Fixtures.CreateJournalEntry();
            //Act
            var curCrate = _journalEntry.GetAccountingTransactionData(curJournalEntry);
            //Assert General Data
            Assert.AreEqual("DocNu1", curCrate.Name);
            Assert.AreEqual(DateTime.UtcNow.Date, curCrate.TransactionDate);
            //Assert First Line
            Assert.AreEqual("36", curCrate.FinancialLines[0].AccountId);
            Assert.AreEqual("Accumulated Depreciation", curCrate.FinancialLines[0].AccountName);
            Assert.AreEqual("100", curCrate.FinancialLines[0].Amount.ToString());
            Assert.AreEqual(PostingTypeEnum.Debit.ToString(), curCrate.FinancialLines[0].DebitOrCredit);
            //Assert Second Line
            Assert.AreEqual("36", curCrate.FinancialLines[1].AccountId);
            Assert.AreEqual("Accumulated Depreciation", curCrate.FinancialLines[1].AccountName);
            Assert.AreEqual("100", curCrate.FinancialLines[1].Amount.ToString());
            Assert.AreEqual(PostingTypeEnum.Credit.ToString(), curCrate.FinancialLines[1].DebitOrCredit);
        }
    }
}