using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects
{
    public class FileDTO
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; }
        public string CloudStorageUrl { get; set; }
        public string Tags { get; set; }
    }
}
