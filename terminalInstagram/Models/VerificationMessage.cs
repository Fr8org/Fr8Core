using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalInstagram.Models
{
    public class VerificationMessage
    {
        public string Mode { get; set; }
        public string Challenge { get; set; }
        public string VerifyToken { get; set; }
    }
}