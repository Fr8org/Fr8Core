using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    class StandardAccountingTransactionDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("financialLines")]
        public List<FinancialLineDTO> FinancialLines { get; set; }
        [JsonProperty("transactionDate")]
        public DateTime TransactionDate { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
    }
}
