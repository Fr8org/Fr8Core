using DocuSign.Integrations.Client;
using Hub.Services;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using Account = DocuSign.Integrations.Client.Account;

namespace terminalDocuSign.Tests
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
        //DocuSignAccount LoginDocusign(DocuSignAccount account, RestSettings restSettings);

        /// <summary>
        /// Create envelope wrapper with fill it with data, and return it back.
        /// </summary>
        /// <param name="account">Docusign account that includes login info.</param>
        /// <param name="envelope">Docusign envelope.</param>
        /// <param name="fullPathToExampleDocument">Full file path to document that will be signed.</param>
        /// <param name="tabCollection">Docusign tab collection.</param>
        /// <returns>Envelope of Docusign.</returns>
        //DocuSignEnvelope CreateAndFillEnvelope(Account account, DocuSignEnvelope envelope, string fullPathToExampleDocument, TabCollection tabCollection);
    }
}