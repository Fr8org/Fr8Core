using System;

using DocusignApiWrapper.Interfaces;

using DocuSign.Integrations.Client;

namespace DocusignApiWrapper
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
                throw new ApplicationException("Please, check your docusign credential info and integrator key.");
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
