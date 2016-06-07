using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Constants;

namespace fr8.Infrastructure.Data.Manifests
{
    public class DocuSignTemplateCM : Manifest
    {
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Body { get; set; }
        public string Name { get; set; }
        public DocuSignTemplateCM()
            : base(MT.DocuSignTemplate)
        {

        }
    }
}
