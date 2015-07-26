using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class EnvelopeDataDO : BaseDO
    {
        public string Name { get; set; }
        public string TabId { get; set; }
        public string RecipientId { get; set; }
        public string EnvelopeId { get; set; }
        public string Value { get; set; }
    }
}
