using System;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using Hub.Services;
using terminalDocuSign.Services;
using terminalDocuSign.Infrastructure;

using Account = DocuSign.Integrations.Client.Account;


namespace terminalDocuSign.Tests
{
    /// <summary>
    /// Docusign .net api wrapper class.
    /// </summary>
    public class DocusignApiHelper : IDocusignApiHelper
    {
        /// <summary>
        /// Programmatically login to DocuSign with Docusign account.
        /// ( Please watch your firewall. It's actualy going to docusign server. )
        /// </summary>
        /// <param name="account">Docusign account.</param>
        /// <param name="restSettings">Docusign restsettings instance.</param>
        /// <returns>Logged account object ( Docusign.Integrations.Client.Account ).</returns>
        public DocuSignAccount LoginDocusign(DocuSignAccount account, RestSettings restSettings)
        {
            // make the Login API call
            bool result = account.Login();

            if (!result)
            {
                string errorText = "There is something wrong with contacting api. ";

                errorText = GetRestErrorAsSerialized(account, errorText);
                errorText = SerializeParametersAndFillErrorText(account, restSettings, errorText);

                throw new ApplicationException(errorText);
            }

            return account;
        }

        /// <summary>
        /// Create envelope with fill it with data, and return it back.
        /// </summary>
        /// <param name="account">Docusign account that includes login info.</param>
        /// <param name="envelope">Docusign envelope.</param>
        /// <param name="fullPathToExampleDocument">Full file path to document that will be signed.</param>
        /// <param name="tabCollection">Docusign tab collection.</param>
        /// <returns>Envelope of Docusign.</returns>
        //public DocuSignEnvelope CreateAndFillEnvelope(Account account, DocuSignEnvelope envelope, string fullPathToExampleDocument, TabCollection tabCollection)
        //{
        //    // create a new DocuSign envelope...
        //    envelope.Create(fullPathToExampleDocument);

        //    //populate it with some Tabs with values. Example "Amount" is a text field with value "45".
        //    envelope.AddTabs(tabCollection);

        //    return envelope;
        //}

        #region [ private methods ]
        private static string GetRestErrorAsSerialized(Account account, string errorText)
        {
            if (account.RestError != null)
            {
                if (!string.IsNullOrEmpty(account.RestError.Serialize()))
                {
                    errorText += "Error Detail: " + account.RestError.Serialize();
                }
            }
            return errorText;
        }

        private static string SerializeParametersAndFillErrorText(Account account, RestSettings restSettings, string errorText)
        {
            string accountSerialized = JsonConvert.SerializeObject(account);
            string restSettingsSerialized = JsonConvert.SerializeObject(restSettings);

            if (!string.IsNullOrEmpty(accountSerialized))
            {
                errorText += "Account: " + accountSerialized;
            }

            if (!string.IsNullOrEmpty(accountSerialized))
            {
                errorText += "RestSettings: " + restSettingsSerialized;
            }
            return errorText;
        }
        #endregion
    }
}
