using Data.Entities;
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
            var salesForceManager = new SalesforceManager();

            return await salesForceManager.DeleteObject(objectName, objectId, new Data.Entities.AuthorizationTokenDO { Token = authTokenDTO.Token, AdditionalAttributes = authTokenDTO.AdditionalAttributes });
        }

        public static async Task<bool> DeleteObject(AuthorizationTokenDO authTokenDO, string objectName, string objectId)
        {
            return await new SalesforceManager().DeleteObject(objectName, objectId, authTokenDO);
        }
    }
}
