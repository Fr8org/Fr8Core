using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Configuration.Azure;

namespace terminalBox.Services
{
    public static class BoxHelpers
    {
        public static string ClientId = CloudConfigurationManager.GetSetting("BoxClientId");
        public static string Secret = CloudConfigurationManager.GetSetting("BoxSecret");
        public static string RedirectUri = CloudConfigurationManager.GetSetting("BoxCallbackUrlsDomain") + "AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalBox&terminalVersion=1";
    }
}