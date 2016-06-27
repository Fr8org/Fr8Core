using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardEmailMessageCM : Manifest
    {
        public StandardEmailMessageCM()
            : base(MT.StandardEmailMessage)
        {

        }

        public string MessageID { get; set; }
        
        public string Subject { get; set; }

        public string HtmlText { get; set; }

        public string PlainText { get; set; }

        public string DateReceived { get; set; }
        
        public string EmailFromName { get; set; }

        public string EmailFrom { get; set; }
    }
}
