using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PollingDataDTO
    {
        public PollingDataDTO()
        {
            JobId = Guid.NewGuid().ToString();
        }

        
        public bool Result { get; set; }
        public bool TriggerImmediately { get; set; }
        public string JobId { get; set; }
        public string ExternalAccountId { get; set; }
        public string Fr8AccountId { get; set; }
        public string PollingIntervalInMinutes { get; set; }
        public string Payload { get; set; }
    }
}
