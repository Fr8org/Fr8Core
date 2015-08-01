using System;
using System.Configuration;

using DocusignApiWrapper.Domains;
using DocusignApiWrapper.Interfaces;

using DocuSign.Integrations.Client;

namespace DocusignApiWrapper
{
    /// <summary>
    /// Docusign .net api wrapper class.
    /// </summary>
    public class DocusignApiHelper : IDocusignApiHelper
    {
        private readonly BasicRestSettings basicRestSettings;

        public DocusignApiHelper()
        {
            basicRestSettings = new BasicRestSettings
                                {
                                    IntegratorKey = ConfigurationManager.AppSettings["Docusign_IntegratorKey"],
                                    DocuSignAddress = ConfigurationManager.AppSettings["Docusign_DocuSignAddress"],
                                    WebServiceUrlRestVersionPart = ConfigurationManager.AppSettings["Docusign_WebServiceUrlRestVersionPart"]
                                };
        }

        /// <summary>
        /// Programmatically login to DocuSign with Docusign account.
        /// ( Please watch your firewall. It's actualy going to docusign server. )
        /// </summary>
        /// <returns>Logged account object ( Docusign.Integrations.Client.Account ).</returns>
        public Account LoginDocusign(Account account)
        {
            // configure application's integrator key and webservice url
            RestSettings.Instance.IntegratorKey = basicRestSettings.IntegratorKey;
            RestSettings.Instance.DocuSignAddress = basicRestSettings.DocuSignAddress;
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + basicRestSettings.WebServiceUrlRestVersionPart;

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
