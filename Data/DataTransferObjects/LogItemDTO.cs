using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects
{
    public class LogItemDTO
    {
        public string Name { get; set; }

        public string PrimaryCategory { get; set; }

        public string SecondaryCategory { get; set; }

        public string Activity { get; set; }

        public string Data { get; set; }

        public bool IsLogged { get; set; }

        public string Status { get; set; }

        public string CustomerId { get; set; }

        public string ObjectId { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime CreateDate { get; set; }

        public string Discriminator { get; set; }

        public string Priority { get; set; }

        public string Manufacturer { get; set; }

        public string Type { get; set; }
    }
}
