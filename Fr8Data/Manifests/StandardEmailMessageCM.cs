using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.Manifests
{
    public class StandardEmailMessageCM : Manifest
    {
        public StandardEmailMessageCM()
            : base(Constants.MT.StandardEmailMessage)
        {

        }

        public string MessageID { get; set; }

        public string References { get; set; }
        public string Subject { get; set; }

        public string HtmlText { get; set; }

        public string PlainText { get; set; }

        public DateTime DateReceived { get; set; }

        public string EmailStatus { get; set; }

        public string EmailFromName { get; set; }
    }
}
