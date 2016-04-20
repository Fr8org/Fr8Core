using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;

namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceManager
    {
        Task<string> Create(SalesforceObjectType type, IDictionary<string, object> @object, AuthorizationTokenDO authTokenDO);

        Task<StandardTableDataCM> Query(SalesforceObjectType type, IEnumerable<string> propertiesToRetrieve, string filter, AuthorizationTokenDO authTokenDO);
        
        Task<List<FieldDTO>> GetProperties(SalesforceObjectType type, AuthorizationTokenDO authTokenDO, bool updatableOnly = false);

        T CreateSalesforceDTO<T>(ActivityDO activity, PayloadDTO payload) where T : new();

        Task<string> PostToChatter(string message, string parentObjectId, AuthorizationTokenDO authTokenDO);

        IEnumerable<FieldDTO> GetSalesforceObjectTypes(SalesforceObjectOperations filterByOperations = SalesforceObjectOperations.All, SalesforceProperties filterByProperties = SalesforceProperties.All);

        Task<bool> Delete(string salesforceObjectName, string objectId, AuthorizationTokenDO authTokenDO);
    }


}