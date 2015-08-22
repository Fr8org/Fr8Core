using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class ErrorDTO
    {
        public string StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
    }
}
