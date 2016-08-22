using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class StandardAccountingTransactionDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("financialLines")]
        public List<FinancialLineDTO> FinancialLines { get; set; }
        [JsonProperty("transactionDate")]
        public DateTime TransactionDate { get; set; }
        [JsonProperty("Memo")]
        public string Memo { get; set; }
    }
}
