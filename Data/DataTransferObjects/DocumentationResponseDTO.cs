using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.DataTransferObjects
{
    public class DocumentationResponseDTO
    {
        public string Name { get; set; }
        public double Version { get; set; }
        //TODO: To be changed with another type
        public string Terminal { get; set; }
        //This field is to hold an HTML
        public string Body { get; set; }
    }

}
