
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        /// <summary>
        /// This is test RestSettins for unit tests.
        /// </summary>
        /// <returns></returns>
        public static RestSettings TestRestSettings1()
        {
            // configure application's integrator key and webservice url
            RestSettings.Instance.IntegratorKey = "TEST-34d0ac9c-89e7-4acc-bc1d-24d6cfb867f2";
            RestSettings.Instance.DocuSignAddress = "https://demo.docusign.net";
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";

            return RestSettings.Instance;
        }
    }
}
