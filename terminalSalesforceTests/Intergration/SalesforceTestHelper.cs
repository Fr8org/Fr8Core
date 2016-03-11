using Data.Interfaces.DataTransferObjects;
using Salesforce.Force;
using System.Threading.Tasks;
using terminalSalesforce.Services;

namespace terminalSalesforceTests.Intergration
{
    public static class SalesforceTestHelper
    {
        public static async Task<bool> DeleteObject(AuthorizationTokenDTO authTokenDTO, string objectName, string objectId)
        {
            string instanceUrl, apiVersion;

            var salesForceManager = new SalesforceManager();

            salesForceManager.ParseAuthToken(authTokenDTO.AdditionalAttributes, out instanceUrl, out apiVersion);

            return await new ForceClient(instanceUrl, authTokenDTO.Token, apiVersion).DeleteAsync(objectName, objectId);
        }
    }
}
