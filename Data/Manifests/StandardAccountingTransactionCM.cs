using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8Data.Manifests
{
    public class StandardAccountingTransactionCM : Manifest
    {
        public List<StandardAccountingTransactionDTO> AccountingTransactions{ get; set; }
        public StandardAccountingTransactionCM(): base(Constants.MT.StandardAccountTransaction)
        {
        }
        public static void Validate(StandardAccountingTransactionCM crate )
        {
            if (crate.AccountingTransactions == null)
            {
                throw new NullReferenceException("AccountingTransactionDTOList is null");
            }
            if (crate.AccountingTransactions.Count == 0)
            {
                throw new Exception("No Transactions in the AccountingTransactionDTOList");
            }
        }
        public static void ValidateAccountingTransation(StandardAccountingTransactionDTO curAccountingTransactionDtoTransactionDTO)
        {
            if (curAccountingTransactionDtoTransactionDTO == null)
            {
                throw new ArgumentNullException("No StandardAccountingTransationDTO provided");
            }
            if (curAccountingTransactionDtoTransactionDTO.FinancialLines == null
                || curAccountingTransactionDtoTransactionDTO.FinancialLines.Count == 0
                || curAccountingTransactionDtoTransactionDTO.TransactionDate == null)
            {
                throw new Exception("No Financial Lines or Transaction Date Provided");
            }
            foreach (var curFinLineDTO in curAccountingTransactionDtoTransactionDTO.FinancialLines)
            {
                ValidateFinancialLineDTO(curFinLineDTO);
            }
        }

        private static void ValidateFinancialLineDTO(FinancialLineDTO finLineDTO)
        {
            if (finLineDTO.AccountId == null || finLineDTO.AccountName == null)
            {
                throw new Exception("Some Account Data is Missing");
            }
            if (finLineDTO.Amount == null)
            {
                throw new Exception("Amount is missing");
            }
            if (finLineDTO.DebitOrCredit == null)
            {
                throw new Exception("Debit/Credit information is missing");
            }
        }
    }
}
