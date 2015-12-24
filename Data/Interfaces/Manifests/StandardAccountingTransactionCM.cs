using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

namespace Data.Interfaces.Manifests
{
    public class StandardAccountingTransactionCM : Manifest
    {
        public List<StandardAccountingTransactionDTO> AccountingTransactions{ get; set; }
        public StandardAccountingTransactionCM()
            : base(Constants.MT.StandardAccountTransaction)
        {
        }
    }
}
