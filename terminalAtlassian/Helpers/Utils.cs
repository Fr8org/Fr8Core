using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalAtlassian.Helpers
{
    public static class Utils
    {
        private const string HttpSchema = "http://";

        private const string HttpSecuredSchema = "https://";
        public static CredentialsDTO EnforceDomainSchema(this CredentialsDTO credentials)
        {
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.Domain))
            {
                return credentials;
            }
            if (credentials.Domain.StartsWith(HttpSchema, StringComparison.InvariantCultureIgnoreCase))
            {
                credentials.Domain = credentials.Domain.Remove(0, HttpSchema.Length);
            }
            if (!credentials.Domain.StartsWith(HttpSecuredSchema, StringComparison.InvariantCultureIgnoreCase))
            {
                credentials.Domain = credentials.Domain.Insert(0, HttpSecuredSchema);
            }
            return credentials;
        }
    }
}