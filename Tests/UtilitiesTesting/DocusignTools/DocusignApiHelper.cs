using System;

using DocuSign.Integrations.Client;

using UtilitiesTesting.DocusignTools.Interfaces;

namespace UtilitiesTesting.DocusignTools
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
        public Account LoginDocusign(Account account, RestSettings restSettings)
        {
            // make the Login API call
            bool result = account.Login();

            if (!result)
            {
                string errorText = "There is something wrong with contacting api. " +
                                   "Please, check your docusign credential info and integrator key first. " +
                                   "Error: " + account.RestError + " " +
                                   "Trace: " + account.RestTrace;

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
        public Envelope CreateAndFillEnvelope(Account account, Envelope envelope, string fullPathToExampleDocument, TabCollection tabCollection)
        {
            // create a new DocuSign envelope...
            envelope.Create(fullPathToExampleDocument);

            //populate it with some Tabs with values. Example "Amount" is a text field with value "45".
            envelope.AddTabs(tabCollection);

            return envelope;
        }
    }
}
