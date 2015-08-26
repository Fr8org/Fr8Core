using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class FieldDefinitionDTO
    {
        public string Name { get; set; }
        public string Required { get; set; }
        public string Value { get; set; }
        public string FieldLabel { get; set; }
    }
}
