using System;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using NUnit.Framework;
using terminalQuickBooks.Actions;
using terminalQuickBooks.Services;
using UtilitiesTesting;

namespace terminalQuickBooks.Tests.Services
{
    public class ObjectMapperTests : BaseTest
    {
        [Test]
        public void CreateJournalEntry_ConvertsPayloadDataToStandardAccountingTransactionCM()
        {
            //Assign
            var curPayloadDataCrate = Fixtures.Fixtures.GetPayloadDataCM();
            //Act
            var curAccountTransCrate = ObjectMapper.MapPayloadDataToAccountingTransactionCM(curPayloadDataCrate);
            //Assert
            //StandardAccountingTransactionDTO fields assert
            Assert.IsInstanceOf(typeof(StandardAccountingTransactionCM), curAccountTransCrate);
            Assert.AreEqual("Testing Journal Entry", curAccountTransCrate.AccountingTransactionDTO.Memo);
            Assert.AreEqual("DocuNum1", curAccountTransCrate.AccountingTransactionDTO.Name);
            Assert.AreEqual(DateTime.Parse("11-11-2015"), curAccountTransCrate.AccountingTransactionDTO.TransactionDate);
            //First Financial Line DTO assert
            Assert.AreEqual("Savings", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[0].AccountName);
            Assert.AreEqual("1", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[0].AccountId);
            Assert.AreEqual("Debit", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[0].DebitOrCredit);
            Assert.AreEqual("100", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[0].Amount);
            Assert.AreEqual("Move money from Savings", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[0].Description);
            //Second Financial Line DTO assert
            Assert.AreEqual("Savings", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[1].AccountName);
            Assert.AreEqual("1", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[1].AccountId);
            Assert.AreEqual("Credit", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[1].DebitOrCredit);
            Assert.AreEqual("100", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[1].Amount);
            Assert.AreEqual("Move money to Savings", curAccountTransCrate.AccountingTransactionDTO.FinancialLines[1].Description);
        }
    }
}