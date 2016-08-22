using System;
using Google.Apis.Auth.OAuth2.Responses;
using Google.GData.Client;

namespace terminalGoogle.Services
{
    public class GoogleAuthHelper
    {
        public static bool IsTokenInvalidation(Exception ex)
        {
            if (ex is GDataRequestException)
            {
                var gdataException = (GDataRequestException)ex;
                if (!string.IsNullOrEmpty(gdataException.ResponseString)
                    && gdataException.ResponseString.ToUpper().Contains("TOKEN")
                    && (gdataException.ResponseString.ToUpper().Contains("INVALID")
                        || gdataException.ResponseString.ToUpper().Contains("REVOKED")))
                {
                    return true;
                }
            }
            else if (ex is TokenResponseException)
            {
                return true;
            }

            return false;
        }
    }
}