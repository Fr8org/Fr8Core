using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class FinancialLineDTO
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }
        [JsonProperty("accountName")]
        public string AccountName { get; set; }
        [JsonProperty("accountId")]
        public string AccountId { get; set; }
        [JsonProperty("debitOrCredit")]
        public string DebitOrCredit { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
    }
}
