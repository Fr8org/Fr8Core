using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalGoogle.DataTransferObjects
{
    public class GoogleAuthDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
    }
}