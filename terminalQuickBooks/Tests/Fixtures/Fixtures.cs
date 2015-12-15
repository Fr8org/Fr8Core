using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            List<Line> lineList = new List<Line>();

            Line debitLine = new Line();
            debitLine.Description = "nov portion of rider insurance";
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
            creditLine.Description = "nov portion of rider insurance";
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
                DebitOrCredit = "Debit"
            };
            var curSecondLineDTO = new FinancialLineDTO()
            {
                Amount = "100",
                AccountId = "2",
                AccountName = "Account-B",
                DebitOrCredit = "Credit"
            };
            curFinLineDTOList.Add(curFirstLineDTO);
            curFinLineDTOList.Add(curSecondLineDTO);
            return new StandardAccountingTransactionCM()
            {
                Description = "That it the test crate",
                FinancialLines = curFinLineDTOList,
                Name = "Code1",
                TransactionDate = DateTime.Parse("2015-12-15")
            };
        }
    }
}