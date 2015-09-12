using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class EnvelopeDataDTO
    {
        public string Name { get; set; }

        public string TabId { get; set; }

        public string RecipientId { get; set; }

        public string EnvelopeId { get; set; }

        public string Value { get; set; }

        public int DocumentId { get; set; }

        public string Type { get; set; }
    }
}
