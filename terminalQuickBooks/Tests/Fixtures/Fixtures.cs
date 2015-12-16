using System;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Intuit.Ipp.Data;

namespace terminalQuickBooks.Tests.Fixtures
{
    public static class Fixtures
    {
        public static Intuit.Ipp.Data.JournalEntry CreateJournalEntry()
        {
            var journalEntry = new Intuit.Ipp.Data.JournalEntry();
            journalEntry.Adjustment = true;
            journalEntry.AdjustmentSpecified = true;

            journalEntry.DocNumber = "DocNu1";
            journalEntry.TxnDate = DateTime.UtcNow.Date;
            journalEntry.TxnDateSpecified = true;
            journalEntry.HomeTotalAmt = 100;
            journalEntry.HomeTotalAmtSpecified = true;
            journalEntry.TotalAmt = 100;
            journalEntry.TotalAmtSpecified = true;
            journalEntry.PrivateNote = "This is the test Journal Entry";
            List<Line> lineList = new List<Line>();

            Line debitLine = new Line();
            debitLine.Description = "That is the first line description";
            debitLine.Amount = new Decimal(100.00);
            debitLine.AmountSpecified = true;
            debitLine.DetailType = LineDetailTypeEnum.JournalEntryLineDetail;
            debitLine.DetailTypeSpecified = true;
            JournalEntryLineDetail journalEntryLineDetail = new JournalEntryLineDetail();
            journalEntryLineDetail.PostingType = PostingTypeEnum.Debit;
            journalEntryLineDetail.PostingTypeSpecified = true;
            journalEntryLineDetail.AccountRef = new ReferenceType() { name = "Accumulated Depreciation", Value = "36" };
            debitLine.AnyIntuitObject = journalEntryLineDetail;
            lineList.Add(debitLine);

            Line creditLine = new Line();
            creditLine.Description = "That is the second line description";
            creditLine.Amount = new Decimal(100.00);
            creditLine.AmountSpecified = true;
            creditLine.DetailType = LineDetailTypeEnum.JournalEntryLineDetail;
            creditLine.DetailTypeSpecified = true;
            JournalEntryLineDetail journalEntryLineDetailCredit = new JournalEntryLineDetail();
            journalEntryLineDetailCredit.PostingType = PostingTypeEnum.Credit;
            journalEntryLineDetailCredit.PostingTypeSpecified = true;
            journalEntryLineDetailCredit.AccountRef = new ReferenceType() { name = "Accumulated Depreciation", Value = "36" };
            creditLine.AnyIntuitObject = journalEntryLineDetailCredit;
            lineList.Add(creditLine);

            journalEntry.Line = lineList.ToArray();
            return journalEntry;
        }

        public static StandardAccountingTransactionCM GetAccountingTransactionCM()
        {
            var curFinLineDTOList = new List<FinancialLineDTO>();
            var curFirstLineDTO = new FinancialLineDTO()
            {
                Amount = "100",
                AccountId = "1",
                AccountName = "Account-A",
                DebitOrCredit = "Debit",
                Description = "Move money to Accout-B"
            };
            var curSecondLineDTO = new FinancialLineDTO()
            {
                Amount = "100",
                AccountId = "2",
                AccountName = "Account-B",
                DebitOrCredit = "Credit",
                Description = "Move money from Accout-A"
            };
            curFinLineDTOList.Add(curFirstLineDTO);
            curFinLineDTOList.Add(curSecondLineDTO);
            
            
            var curAccoutingTransactionDTO = new StandardAccountingTransactionDTO()
            {
                Memo = "That is the test crate",
                FinancialLines = curFinLineDTOList,
                Name = "Code1",
                TransactionDate = DateTime.Parse("2015-12-15")
            };
            var curCrate = new StandardAccountingTransactionCM()
            {
                AccountingTransactionDTO = curAccoutingTransactionDTO
            };
            return curCrate;
        }
    }
}