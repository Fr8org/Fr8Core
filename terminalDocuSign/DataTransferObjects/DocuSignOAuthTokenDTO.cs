using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalDocuSign.DataTransferObjects
{
    public class DocuSignOAuthTokenDTO
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }


    }
}