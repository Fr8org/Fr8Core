using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class HistoryItemDTO
    {
        public string Activity { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public string Fr8UserId { get; set; }
        public string Data { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public string ObjectId { get; set; }
        public string Component { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Status { get; set; }
    }
}
