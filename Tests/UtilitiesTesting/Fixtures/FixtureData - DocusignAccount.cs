using System;

using DocuSign.Integrations.Client;

using NUnit.Framework;

namespace UtilitiesTesting.Fixtures
{
    public class DocusignAccount
    {
        /// <summary>
        /// Programmatically create an Envelope in DocuSign in the developer sandbox account.
        /// ( Please watch your firewall. It's actualy going to demo docusign server. )
        /// </summary>
        /// <returns>Logged account object ( Docusign.Integrations.Client.Account ).</returns>
        public static Account LoginDocusign()
        {
            // configure application's integrator key and webservice url
            RestSettings.Instance.IntegratorKey = "TEST-34d0ac9c-89e7-4acc-bc1d-24d6cfb867f2";
            RestSettings.Instance.DocuSignAddress = "http://demo.docusign.net";
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";

            // credentials for sending account
            Account account = new Account
                              {
                                  Email = "hello@orkan.com",
                                  Password = "q.12345R",
                                  AccountIdGuid = Guid.Parse("06e1493c-75be-428a-806e-7480ccff823a"),
                                  AccountId = "1124624",
                                  UserName = "ORKAN ARIKAN"
                              };

            // make the Login API call
            bool result = account.Login();

            Assert.IsTrue(result, "We login to docusign. If this is false, check your credential info and integrator key.");

            return account;
        }
    }
}