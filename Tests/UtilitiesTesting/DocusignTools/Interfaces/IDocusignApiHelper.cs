using Data.Wrappers;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.DocusignTools.Interfaces
{
    public interface IDocusignApiHelper
    {
        /// <summary>
        /// Programmatically login to DocuSign with Docusign account.
        /// ( Please watch your firewall. It's actualy going to docusign server. )
        /// </summary>
        /// <param name="account">Docusign account.</param>
        /// <param name="restSettings">Docusign restsettings instance.</param>
        /// <returns>Logged account object ( Docusign.Integrations.Client.Account ).</returns>
        Account LoginDocusign(Account account, RestSettings restSettings);

        /// <summary>
        /// Create envelope wrapper with fill it with data, and return it back.
        /// </summary>
        /// <param name="account">Docusign account that includes login info.</param>
        /// <param name="envelope">Docusign envelope.</param>
        /// <param name="fullPathToExampleDocument">Full file path to document that will be signed.</param>
        /// <param name="tabCollection">Docusign tab collection.</param>
        /// <returns>Envelope of Docusign.</returns>
        DocuSignEnvelope CreateAndFillEnvelope(Account account, DocuSignEnvelope envelope, string fullPathToExampleDocument, TabCollection tabCollection);
    }
}