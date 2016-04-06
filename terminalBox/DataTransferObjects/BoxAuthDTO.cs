using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalBox.DataTransferObjects
{
    public class BoxAuthDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
    }
}