using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalFacebook.Models
{
    public class VerificationMessage
    {
        public string Mode { get; set; }
        public int Challenge { get; set; }
        public string VerifyToken { get; set; }
    }
}