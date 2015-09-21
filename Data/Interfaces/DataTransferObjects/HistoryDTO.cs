using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class HistoryDTO
    {
        public int Id { get; set; }
        public string ObjectId { get; set; }
        public string DockyardAccountId { get; set; }
        public string PrimaryCategory { get; set; }
        public string SecondaryCategory { get; set; }
        public string Activity { get; set; }
        public string Data { get; set; }
        public string Status { get; set; }
    }
}
