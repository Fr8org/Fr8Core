using Fr8.Infrastructure.Utilities;
using System;

namespace terminalSalesforce.Services
{
    public class SalesforceAuthHelper
    {
        public static bool IsTokenInvalidation(Exception ex)
        {
            return ex.Message.Contains("expired access/refresh token", StringComparison.InvariantCulture);
        }
    }
}