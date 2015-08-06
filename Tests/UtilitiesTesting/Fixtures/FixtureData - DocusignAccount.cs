using System;

using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static Account TestAccount1()
        {
            // credentials for sending account
            return new Account
                   {
                       Email = "hello@orkan.com",
                       Password = "q.12345R",
                       AccountIdGuid = Guid.Parse("06e1493c-75be-428a-806e-7480ccff823a"),
                       AccountId = "1124624",
                       UserName = "ORKAN ARIKAN"
                   };
        }
    }
}