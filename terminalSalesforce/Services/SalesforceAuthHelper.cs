using System;

namespace terminalSalesforce.Services
{
    public class SalesforceAuthHelper
    {
        public static bool IsTokenInvalidation(Exception ex)
        {
            return ex.Message == "expired access/refresh token";
        }
    }
}